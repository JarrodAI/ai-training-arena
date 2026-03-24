using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for recording and querying battle telemetry data.
/// </summary>
public interface ITelemetryService
{
    /// <summary>Records a telemetry record, encrypting and storing to IPFS.</summary>
    /// <param name="record">The telemetry record to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordAsync(TelemetryRecord record, CancellationToken ct = default);

    /// <summary>Retrieves telemetry records for a specific battle.</summary>
    /// <param name="battleId">The battle ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<TelemetryRecord>> GetRecordsAsync(string battleId, CancellationToken ct = default);
}
