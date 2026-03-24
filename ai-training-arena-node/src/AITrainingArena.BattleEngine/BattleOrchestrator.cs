using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Orchestrates the full battle lifecycle: matchmaking, role assignment,
/// proposer/solver execution, scoring, and Elo updates.
/// </summary>
public sealed class BattleOrchestrator : IBattleOrchestrator
{
    private const int QuestionsPerBattle = 5;
    private const int MaxHopCount = 3;
    private const string LocalPeerId = "local-node";

    private readonly INetworkManager _networkManager;
    private readonly DrZeroEngine _drZeroEngine;
    private readonly ProposerService _proposerService;
    private readonly SolverService _solverService;
    private readonly IBattleRepository _battleRepository;
    private readonly ILogger<BattleOrchestrator> _logger;

    public BattleOrchestrator(
        INetworkManager networkManager,
        DrZeroEngine drZeroEngine,
        ProposerService proposerService,
        SolverService solverService,
        IBattleRepository battleRepository,
        ILogger<BattleOrchestrator> logger)
    {
        _networkManager = networkManager;
        _drZeroEngine = drZeroEngine;
        _proposerService = proposerService;
        _solverService = solverService;
        _battleRepository = battleRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Battle> InitiateBattleAsync(
        string proposerId,
        string solverId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Initiating battle: proposer={ProposerId}, solver={SolverId}",
            proposerId,
            solverId);

        var battle = Battle.Create(proposerId, solverId, BattleRole.Proposer);
        await _battleRepository.SaveAsync(battle, ct);
        return battle;
    }

    /// <inheritdoc />
    public async Task<BattleResult> RunBattleAsync(Guid battleId, CancellationToken ct = default)
    {
        var battle = await _battleRepository.GetByIdAsync(battleId, ct)
            ?? throw new InvalidOperationException($"Battle {battleId} not found.");

        _logger.LogInformation("Running battle {BattleId}", battleId);
        battle.Start();

        var rounds = battle.LocalRole == BattleRole.Proposer
            ? await ExecuteAsProposerAsync(battle, ct)
            : await ExecuteAsSolverAsync(battle, ct);

        foreach (var round in rounds)
        {
            battle.AddRound(round);
        }

        battle.BeginScoring();
        var result = CalculateFinalScores(battle);
        battle.Complete(result);
        await _battleRepository.SaveAsync(battle, ct);

        _logger.LogInformation(
            "Battle {BattleId} completed. Winner: {Winner}",
            battleId,
            result.Winner);

        return result;
    }

    /// <inheritdoc />
    public async Task<BattleResult?> ExecuteBattleAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Searching for opponent...");
        var opponentId = await _networkManager.FindOpponentAsync(
            AgentClass.A,
            EloRating.DefaultRating,
            ct);

        if (opponentId is null)
        {
            _logger.LogInformation("No opponent found, skipping battle");
            return null;
        }

        _logger.LogInformation("Found opponent: {OpponentId}", opponentId);

        var role = DetermineRole();
        var (proposerId, solverId) = role == BattleRole.Proposer
            ? (LocalPeerId, opponentId)
            : (opponentId, LocalPeerId);

        var battle = Battle.Create(proposerId, solverId, role);
        await _battleRepository.SaveAsync(battle, ct);

        battle.Start();

        var rounds = role == BattleRole.Proposer
            ? await ExecuteAsProposerAsync(battle, ct)
            : await ExecuteAsSolverAsync(battle, ct);

        foreach (var round in rounds)
        {
            battle.AddRound(round);
        }

        battle.BeginScoring();
        var result = CalculateFinalScores(battle);
        battle.Complete(result);
        await _battleRepository.SaveAsync(battle, ct);

        _logger.LogInformation(
            "Battle {BattleId} completed. Winner: {Winner}, ProposerScore: {PScore}, SolverScore: {SScore}",
            battle.BattleId,
            result.Winner,
            result.ProposerScore,
            result.SolverScore);

        return result;
    }

    /// <inheritdoc />
    public async Task<Battle?> GetBattleAsync(Guid battleId, CancellationToken ct = default)
    {
        return await _battleRepository.GetByIdAsync(battleId, ct);
    }

    /// <inheritdoc />
    public async Task CancelBattleAsync(Guid battleId, CancellationToken ct = default)
    {
        var battle = await _battleRepository.GetByIdAsync(battleId, ct)
            ?? throw new InvalidOperationException($"Battle {battleId} not found.");

        _logger.LogWarning("Cancelling battle {BattleId}", battleId);
        battle.Cancel();
        await _battleRepository.SaveAsync(battle, ct);
    }

    /// <summary>
    /// Executes the proposer role: generates questions, sends to solver, scores answers.
    /// </summary>
    private async Task<List<QuestionAnswerResult>> ExecuteAsProposerAsync(
        Battle battle,
        CancellationToken ct)
    {
        _logger.LogInformation("Executing as PROPOSER for battle {BattleId}", battle.BattleId);
        var rounds = new List<QuestionAnswerResult>();

        for (var i = 0; i < QuestionsPerBattle; i++)
        {
            ct.ThrowIfCancellationRequested();

            var hopCount = Random.Shared.Next(1, MaxHopCount + 1);
            var question = await _proposerService.GenerateMultiHopQuestionAsync(hopCount, ct);
            var startTime = DateTime.UtcNow;
            var solverAnswer = await _solverService.SolveQuestionAsync(question, ct);
            var responseTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            var isCorrect = DrZeroEngine.VerifyAnswer(question, solverAnswer);
            var difficulty = DrZeroEngine.CalculateDifficulty(question, solverAnswer, isCorrect);

            var round = new QuestionAnswerResult(
                QuestionText: question.Text,
                ExpectedAnswer: question.ExpectedAnswer,
                SolverAnswer: solverAnswer,
                IsCorrect: isCorrect,
                ResponseTimeMs: responseTimeMs,
                DifficultyScore: difficulty,
                HopCount: question.HopCount,
                SearchQueries: []);

            rounds.Add(round);
            _logger.LogDebug(
                "Round {Round}: correct={Correct}, difficulty={Difficulty}",
                i + 1,
                isCorrect,
                difficulty);
        }

        return rounds;
    }

    /// <summary>
    /// Executes the solver role: receives questions and generates answers.
    /// In a real P2P scenario, questions arrive from the network; here we self-generate for testing.
    /// </summary>
    private async Task<List<QuestionAnswerResult>> ExecuteAsSolverAsync(
        Battle battle,
        CancellationToken ct)
    {
        _logger.LogInformation("Executing as SOLVER for battle {BattleId}", battle.BattleId);
        var rounds = new List<QuestionAnswerResult>();

        for (var i = 0; i < QuestionsPerBattle; i++)
        {
            ct.ThrowIfCancellationRequested();

            var hopCount = Random.Shared.Next(1, MaxHopCount + 1);
            var question = await _drZeroEngine.GenerateQuestionAsync(hopCount, ct);
            var startTime = DateTime.UtcNow;
            var answer = await _solverService.SolveQuestionAsync(question, ct);
            var responseTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            var isCorrect = DrZeroEngine.VerifyAnswer(question, answer);
            var difficulty = DrZeroEngine.CalculateDifficulty(question, answer, isCorrect);

            var round = new QuestionAnswerResult(
                QuestionText: question.Text,
                ExpectedAnswer: question.ExpectedAnswer,
                SolverAnswer: answer,
                IsCorrect: isCorrect,
                ResponseTimeMs: responseTimeMs,
                DifficultyScore: difficulty,
                HopCount: question.HopCount,
                SearchQueries: []);

            rounds.Add(round);
        }

        return rounds;
    }

    /// <summary>
    /// Randomly assigns the local node as Proposer or Solver.
    /// </summary>
    private static BattleRole DetermineRole()
    {
        return Random.Shared.Next(2) == 0 ? BattleRole.Proposer : BattleRole.Solver;
    }

    /// <summary>
    /// Calculates final scores for the battle.
    /// Proposer score = avgDifficulty * 0.6 + questionCount * 0.4.
    /// Solver score = correctAnswers * 1.5.
    /// </summary>
    private static BattleResult CalculateFinalScores(Battle battle)
    {
        var totalQuestions = battle.Rounds.Count;
        var correctAnswers = battle.Rounds.Count(r => r.IsCorrect);
        var avgDifficulty = totalQuestions > 0
            ? battle.Rounds.Average(r => r.DifficultyScore)
            : 0m;

        var proposerScore = avgDifficulty * 0.6m + totalQuestions * 0.4m;
        var solverScore = correctAnswers * 1.5m;

        var winner = proposerScore >= solverScore
            ? battle.ProposerId
            : battle.SolverId;

        var totalReward = 0.041m;
        var burnAmount = totalReward * 0.02m;
        var netReward = totalReward - burnAmount;
        var winnerShare = netReward * 0.6m;
        var loserShare = netReward * 0.4m;

        var proposerReward = winner == battle.ProposerId ? winnerShare : loserShare;
        var solverReward = winner == battle.SolverId ? winnerShare : loserShare;

        return new BattleResult(
            Winner: winner,
            ProposerScore: proposerScore,
            SolverScore: solverScore,
            TotalQuestions: totalQuestions,
            CorrectAnswers: correctAnswers,
            AvgDifficulty: avgDifficulty,
            ProposerReward: proposerReward,
            SolverReward: solverReward,
            BurnAmount: burnAmount,
            TelemetryIpfsHash: null);
    }
}
