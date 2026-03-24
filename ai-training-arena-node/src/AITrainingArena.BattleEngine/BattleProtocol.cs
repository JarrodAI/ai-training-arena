using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Battle-level protocol layer over a BattleConnection.
/// Handles typed message send/receive with 2-minute per-exchange timeout.
/// Implements the /arena/battle/1.0.0 protocol from master-chapters.md Chapter N-2.
///
/// Protocol flow:
///   Proposer                    Solver
///   -------                     ------
///   BattleStart  ─────────────► BattleStart
///   Question[1]  ─────────────► (process)
///                ◄───────────── Answer[1]
///   ...repeat N times...
///   BattleEnd    ─────────────► BattleEnd
/// </summary>
public sealed class BattleProtocol
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    private readonly ILogger<BattleProtocol> _logger;

    public BattleProtocol(ILogger<BattleProtocol> logger)
    {
        _logger = logger;
    }

    /// <summary>Send a question to the solver over the battle connection.</summary>
    public async Task SendQuestionAsync(
        BattleConnection conn,
        Question question,
        CancellationToken ct = default)
    {
        var msg = BattleMessage.Create(BattleMessageType.Question, conn.BattleId, new
        {
            text = question.Text,
            hopCount = question.HopCount,
            difficultyScore = question.DifficultyScore,
            generatedAt = question.GeneratedAt,
        });

        await conn.SendMessageAsync(msg, ct);
        _logger.LogDebug("Sent question (hops={Hops}) to {Peer}", question.HopCount, conn.OpponentPeerId);
    }

    /// <summary>Receive a question from the proposer. Returns null on timeout.</summary>
    public async Task<Question?> ReceiveQuestionAsync(
        BattleConnection conn,
        CancellationToken ct = default)
    {
        var msg = await conn.ReceiveMessageAsync(DefaultTimeout, ct);
        if (msg is null || msg.Type != BattleMessageType.Question)
            return null;

        var payload = msg.DeserializePayload<QuestionPayload>();
        if (payload is null) return null;

        return new Question
        {
            Text = payload.text,
            HopCount = payload.hopCount,
            ExpectedAnswer = string.Empty, // solver doesn't receive expected answer
            DifficultyScore = payload.difficultyScore,
            HopChain = [],
            GeneratedAt = payload.generatedAt,
        };
    }

    /// <summary>Send the solver's answer to the proposer.</summary>
    public async Task SendAnswerAsync(
        BattleConnection conn,
        string answer,
        CancellationToken ct = default)
    {
        var msg = BattleMessage.Create(BattleMessageType.Answer, conn.BattleId, new { answer });
        await conn.SendMessageAsync(msg, ct);
        _logger.LogDebug("Sent answer to {Peer}", conn.OpponentPeerId);
    }

    /// <summary>Receive the solver's answer. Returns null on timeout.</summary>
    public async Task<string?> ReceiveAnswerAsync(
        BattleConnection conn,
        CancellationToken ct = default)
    {
        var msg = await conn.ReceiveMessageAsync(DefaultTimeout, ct);
        if (msg is null || msg.Type != BattleMessageType.Answer)
            return null;

        var payload = msg.DeserializePayload<AnswerPayload>();
        return payload?.answer;
    }

    /// <summary>Proposer sends final BattleEnd message with result summary.</summary>
    public async Task SendBattleEndAsync(
        BattleConnection conn,
        BattleResult result,
        CancellationToken ct = default)
    {
        var msg = BattleMessage.Create(BattleMessageType.BattleEnd, conn.BattleId, new
        {
            winner = result.Winner,
            proposerScore = result.ProposerScore,
            solverScore = result.SolverScore,
            totalQuestions = result.TotalQuestions,
            correctAnswers = result.CorrectAnswers,
        });

        await conn.SendMessageAsync(msg, ct);
        _logger.LogInformation(
            "Sent BattleEnd to {Peer}. Winner={Winner}",
            conn.OpponentPeerId, result.Winner);
    }

    /// <summary>Solver receives BattleEnd from proposer. Returns null on timeout.</summary>
    public async Task<BattleResult?> ReceiveBattleEndAsync(
        BattleConnection conn,
        CancellationToken ct = default)
    {
        var msg = await conn.ReceiveMessageAsync(DefaultTimeout, ct);
        if (msg is null || msg.Type != BattleMessageType.BattleEnd)
            return null;

        var p = msg.DeserializePayload<BattleEndPayload>();
        if (p is null) return null;

        return new BattleResult(
            Winner: p.winner,
            ProposerScore: p.proposerScore,
            SolverScore: p.solverScore,
            TotalQuestions: p.totalQuestions,
            CorrectAnswers: p.correctAnswers,
            AvgDifficulty: 0,
            ProposerReward: 0,
            SolverReward: 0,
            BurnAmount: 0,
            TelemetryIpfsHash: null);
    }

    /// <summary>Send a heartbeat to detect stale connections.</summary>
    public async Task SendHeartbeatAsync(BattleConnection conn, CancellationToken ct = default)
    {
        var msg = BattleMessage.Heartbeat(conn.BattleId);
        await conn.SendMessageAsync(msg, ct);
    }

    /// <summary>Wait for a heartbeat response. Returns false on timeout.</summary>
    public async Task<bool> WaitForHeartbeatAsync(
        BattleConnection conn,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        var msg = await conn.ReceiveMessageAsync(timeout, ct);
        return msg?.Type == BattleMessageType.Heartbeat;
    }

    // ─── Private payload types ────────────────────────────────────────────────

    private record QuestionPayload(string text, int hopCount, decimal difficultyScore, DateTime generatedAt);
    private record AnswerPayload(string answer);
    private record BattleEndPayload(string winner, decimal proposerScore, decimal solverScore, int totalQuestions, int correctAnswers);
}
