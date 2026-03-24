using AITrainingArena.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AITrainingArena.Tests;

public class BattleMessageTests
{
    [Fact]
    public void BattleMessage_SerializeDeserialize_RoundTrips()
    {
        var id = Guid.NewGuid();
        var original = BattleMessage.Heartbeat(id);
        var bytes = original.Serialize();
        var result = BattleMessage.Deserialize(bytes);
        result.Type.Should().Be(BattleMessageType.Heartbeat);
        result.BattleId.Should().Be(id);
    }

    [Fact]
    public void BattleMessage_Create_WithPayload_Deserializes()
    {
        var id = Guid.NewGuid();
        var msg = BattleMessage.Create(BattleMessageType.Answer, id, new { answer = "Paris" });
        var bytes = msg.Serialize();
        var result = BattleMessage.Deserialize(bytes);
        result.Type.Should().Be(BattleMessageType.Answer);
        result.Payload.Should().NotBeEmpty();
    }

    [Fact]
    public void BattleMessage_TooShortBytes_Throws()
    {
        var act = () => BattleMessage.Deserialize(new byte[10]);
        act.Should().Throw<ArgumentException>();
    }
}
