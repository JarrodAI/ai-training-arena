using System.Text;
using System.Text.Json;

namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Types of messages exchanged over the P2P battle stream.
/// </summary>
public enum BattleMessageType
{
    /// <summary>Proposer sends a question to the solver.</summary>
    Question,
    /// <summary>Solver sends an answer back to the proposer.</summary>
    Answer,
    /// <summary>Sent by both nodes at the start of a battle (role negotiation).</summary>
    BattleStart,
    /// <summary>Sent at the end of a battle with the final result.</summary>
    BattleEnd,
    /// <summary>Keepalive message to detect disconnected peers.</summary>
    Heartbeat,
}

/// <summary>
/// Binary-serializable message for P2P battle communication.
/// Format: [1-byte type][16-byte battleId][8-byte timestamp][4-byte payloadLen][payloadLen bytes]
/// </summary>
public sealed class BattleMessage
{
    public BattleMessageType Type { get; init; }
    public Guid BattleId { get; init; }
    public DateTime Timestamp { get; init; }
    public byte[] Payload { get; init; } = [];

    /// <summary>Serialize this message to a byte array for network transmission.</summary>
    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        w.Write((byte)Type);
        w.Write(BattleId.ToByteArray());
        w.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        w.Write(Payload.Length);
        w.Write(Payload);

        return ms.ToArray();
    }

    /// <summary>Deserialize a BattleMessage from a byte array.</summary>
    /// <exception cref="ArgumentException">If the data is malformed.</exception>
    public static BattleMessage Deserialize(byte[] data)
    {
        if (data.Length < 29)
            throw new ArgumentException($"BattleMessage too short: {data.Length} bytes");

        using var ms = new MemoryStream(data);
        using var r = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        var type = (BattleMessageType)r.ReadByte();
        var battleIdBytes = r.ReadBytes(16);
        var timestampMs = r.ReadInt64();
        var payloadLen = r.ReadInt32();

        if (payloadLen < 0 || payloadLen > 1_000_000)
            throw new ArgumentException($"Invalid payload length: {payloadLen}");

        var payload = r.ReadBytes(payloadLen);

        return new BattleMessage
        {
            Type = type,
            BattleId = new Guid(battleIdBytes),
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime,
            Payload = payload,
        };
    }

    /// <summary>Create a heartbeat message for the given battle.</summary>
    public static BattleMessage Heartbeat(Guid battleId) => new()
    {
        Type = BattleMessageType.Heartbeat,
        BattleId = battleId,
        Timestamp = DateTime.UtcNow,
        Payload = [],
    };

    /// <summary>Create a message with a JSON-serialized payload object.</summary>
    public static BattleMessage Create<T>(BattleMessageType type, Guid battleId, T payload)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        return new BattleMessage { Type = type, BattleId = battleId, Timestamp = DateTime.UtcNow, Payload = json };
    }

    /// <summary>Deserialize the payload as a JSON object.</summary>
    public T? DeserializePayload<T>() =>
        Payload.Length == 0 ? default : JsonSerializer.Deserialize<T>(Payload);
}
