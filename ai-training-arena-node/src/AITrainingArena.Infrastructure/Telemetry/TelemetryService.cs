using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.Telemetry;

public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly List<TelemetryRecord> _records = [];
    private readonly object _lock = new();

    public TelemetryService(ILogger<TelemetryService> logger)
    {
        _logger = logger;
    }

    public Task RecordAsync(TelemetryRecord record, CancellationToken ct = default)
    {
        _logger.LogInformation("Recording telemetry for battle {BattleId}", record.BattleId);
        lock (_lock)
        {
            _records.Add(record);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TelemetryRecord>> GetRecordsAsync(string battleId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching telemetry records for battle {BattleId}", battleId);
        lock (_lock)
        {
            IReadOnlyList<TelemetryRecord> result = _records
                .Where(r => r.BattleId == battleId)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
