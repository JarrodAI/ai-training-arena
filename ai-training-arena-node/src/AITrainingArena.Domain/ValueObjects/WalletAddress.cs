using System.Text.RegularExpressions;

namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Immutable Ethereum/Mantle wallet address value object with format validation.
/// </summary>
public readonly partial record struct WalletAddress
{
    private static readonly Regex AddressPattern = HexAddressRegex();

    /// <summary>The hex-encoded wallet address including 0x prefix.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="WalletAddress"/> from a hex string.
    /// </summary>
    /// <param name="address">The 0x-prefixed, 40-character hex address.</param>
    public WalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Wallet address cannot be empty.", nameof(address));
        if (!AddressPattern.IsMatch(address))
            throw new ArgumentException($"Invalid wallet address format: {address}", nameof(address));
        Value = address;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(WalletAddress address) => address.Value;

    [GeneratedRegex(@"^0x[0-9a-fA-F]{40}$", RegexOptions.Compiled)]
    private static partial Regex HexAddressRegex();
}
