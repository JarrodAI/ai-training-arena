using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Events;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Entities;

/// <summary>
/// Core domain entity representing a battle between two AI agents.
/// Implements a state machine: Pending -> InProgress -> Scoring -> ProofSubmitted -> Completed.
/// </summary>
public class Battle
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Unique identifier for this battle.</summary>
    public Guid BattleId { get; set; }

    /// <summary>Peer ID of the proposer agent.</summary>
    public string ProposerId { get; set; } = string.Empty;

    /// <summary>Peer ID of the solver agent.</summary>
    public string SolverId { get; set; } = string.Empty;

    /// <summary>Role of the local node in this battle.</summary>
    public BattleRole LocalRole { get; set; }

    /// <summary>When the battle started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>When the battle completed (null if still in progress).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Current state in the battle lifecycle.</summary>
    public BattleStatus Status { get; set; } = BattleStatus.Pending;

    /// <summary>Ordered list of question-answer rounds in this battle.</summary>
    public List<QuestionAnswerResult> Rounds { get; set; } = [];

    /// <summary>Final result after scoring (null until Completed).</summary>
    public BattleResult? FinalResult { get; set; }

    /// <summary>Domain events raised by this entity.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Creates a new battle in Pending state.
    /// </summary>
    /// <param name="proposerId">Peer ID of the proposer.</param>
    /// <param name="solverId">Peer ID of the solver.</param>
    /// <param name="localRole">This node's role in the battle.</param>
    public static Battle Create(string proposerId, string solverId, BattleRole localRole)
    {
        return new Battle
        {
            BattleId = Guid.NewGuid(),
            ProposerId = proposerId,
            SolverId = solverId,
            LocalRole = localRole,
            StartedAt = DateTime.UtcNow,
            Status = BattleStatus.Pending
        };
    }

    /// <summary>
    /// Transitions the battle to InProgress state.
    /// </summary>
    public void Start()
    {
        if (Status != BattleStatus.Pending)
            throw new InvalidOperationException($"Cannot start battle in {Status} state.");
        Status = BattleStatus.InProgress;
    }

    /// <summary>
    /// Records a round result during an active battle.
    /// </summary>
    /// <param name="result">The question-answer result for this round.</param>
    public void AddRound(QuestionAnswerResult result)
    {
        if (Status != BattleStatus.InProgress)
            throw new InvalidOperationException($"Cannot add rounds in {Status} state.");
        Rounds.Add(result);
    }

    /// <summary>
    /// Transitions to Scoring state after all rounds complete.
    /// </summary>
    public void BeginScoring()
    {
        if (Status != BattleStatus.InProgress)
            throw new InvalidOperationException($"Cannot begin scoring in {Status} state.");
        Status = BattleStatus.Scoring;
    }

    /// <summary>
    /// Completes the battle with a final result and raises BattleCompleted event.
    /// </summary>
    /// <param name="result">The final battle result.</param>
    public void Complete(BattleResult result)
    {
        if (Status != BattleStatus.Scoring && Status != BattleStatus.ProofSubmitted)
            throw new InvalidOperationException($"Cannot complete battle in {Status} state.");
        FinalResult = result;
        Status = BattleStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        _domainEvents.Add(new BattleCompleted(BattleId, ProposerId, SolverId, result, CompletedAt.Value));
    }

    /// <summary>
    /// Marks the battle proof as submitted to the blockchain.
    /// </summary>
    public void MarkProofSubmitted()
    {
        if (Status != BattleStatus.Scoring)
            throw new InvalidOperationException($"Cannot submit proof in {Status} state.");
        Status = BattleStatus.ProofSubmitted;
    }

    /// <summary>
    /// Cancels the battle due to timeout or disconnect.
    /// </summary>
    public void Cancel()
    {
        if (Status == BattleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed battle.");
        Status = BattleStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the battle as disputed for oracle resolution.
    /// </summary>
    public void Dispute()
    {
        if (Status != BattleStatus.ProofSubmitted && Status != BattleStatus.Completed)
            throw new InvalidOperationException($"Cannot dispute battle in {Status} state.");
        Status = BattleStatus.Disputed;
    }

    /// <summary>
    /// Clears all pending domain events after they have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
