namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Represents an active P2P battle connection to an opponent node.
/// Wraps the underlying network stream for battle-protocol message exchange.
/// </summary>
public sealed class BattleConnection : IAsyncDisposable
{
    /// <summary>LibP2P peer ID of the opponent node.</summary>
    public string OpponentPeerId { get; init; } = string.Empty;

    /// <summary>Battle ID negotiated during the handshake.</summary>
    public Guid BattleId { get; init; } = Guid.NewGuid();

    /// <summary>When the connection was established.</summary>
    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Whether the connection is currently alive.</summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>
    /// Stream for reading from the opponent.
    /// Phase 1: backed by a MemoryStream for local testing.
    /// Phase 2: backed by a LibP2P multiplexed stream.
    /// </summary>
    public Stream ReadStream { get; init; } = Stream.Null;

    /// <summary>Stream for writing to the opponent.</summary>
    public Stream WriteStream { get; init; } = Stream.Null;

    /// <summary>Lock to serialize concurrent writes.</summary>
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Sends a serialized BattleMessage over the write stream.
    /// Thread-safe via internal write lock.
    /// </summary>
    public async Task SendMessageAsync(BattleMessage message, CancellationToken ct = default)
    {
        var bytes = message.Serialize();
        var lengthPrefix = BitConverter.GetBytes(bytes.Length);

        await _writeLock.WaitAsync(ct);
        try
        {
            await WriteStream.WriteAsync(lengthPrefix, ct);
            await WriteStream.WriteAsync(bytes, ct);
            await WriteStream.FlushAsync(ct);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Reads the next BattleMessage from the read stream.
    /// Blocks until a message arrives or the timeout elapses.
    /// </summary>
    public async Task<BattleMessage?> ReceiveMessageAsync(
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            // Read 4-byte length prefix
            var lenBuf = new byte[4];
            await ReadStream.ReadExactlyAsync(lenBuf, cts.Token);

            var payloadLen = BitConverter.ToInt32(lenBuf);
            if (payloadLen <= 0 || payloadLen > 1_048_576) // max 1MB
                return null;

            var payload = new byte[payloadLen];
            await ReadStream.ReadExactlyAsync(payload, cts.Token);
            return BattleMessage.Deserialize(payload);
        }
        catch (OperationCanceledException)
        {
            return null; // timeout or cancelled
        }
    }

    /// <summary>Mark the connection as no longer alive.</summary>
    public void Close()
    {
        IsAlive = false;
    }

    public async ValueTask DisposeAsync()
    {
        IsAlive = false;
        await WriteStream.DisposeAsync();
        await ReadStream.DisposeAsync();
        _writeLock.Dispose();
    }
}
