using System.Security.Cryptography;

namespace Arthea.Core.Social;

/// <summary>Privileged account record. Never serialize or log its credential verifier.</summary>
public sealed class PasswordAccountRecord(string playerId, string encodedVerifier)
{
    public string PlayerId { get; } = playerId;
    [System.Text.Json.Serialization.JsonIgnore]
    public string EncodedVerifier { get; } = encodedVerifier;
    public override string ToString() => "PasswordAccountRecord([REDACTED])";
}

/// <summary>
/// An in-memory verifier for trusted pre-provisioned accounts, implementing the
/// session coordinator's credential contract as "player-id\npassword".
/// No account database, registration, remote login, or network listener is provided.
/// </summary>
public sealed class PasswordAccountAuthenticator : IPlayerAuthenticator
{
    public const int Iterations = 600_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;
    private readonly Dictionary<string, (byte[] Salt, byte[] Hash)> _accounts =
        new(StringComparer.Ordinal);
    private readonly byte[] _dummySalt = RandomNumberGenerator.GetBytes(SaltBytes);
    private readonly byte[] _zeroHash = new byte[HashBytes];

    public PasswordAccountAuthenticator(IEnumerable<PasswordAccountRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        foreach (var record in records)
        {
            ArgumentNullException.ThrowIfNull(record);
            ValidateId(record.PlayerId);
            if (_accounts.Count >= 10_000)
                throw new ArgumentException("Account capacity exceeded.");
            if (!_accounts.TryAdd(record.PlayerId, ParseVerifier(record.EncodedVerifier)))
                throw new ArgumentException("Duplicate account.");
        }
    }

    /// <summary>Use only for trusted, offline provisioning; never a public registration endpoint.</summary>
    public static PasswordAccountRecord Provision(string playerId, string password)
    {
        ValidateId(playerId);
        if (password is null || password.Length is < 12 or > 256 ||
            password.Any(char.IsControl))
            throw new ArgumentException("Password must be 12-256 characters without control characters.");
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
        var encoded = string.Join("$", "v1", "pbkdf2-sha256", Iterations.ToString(
            System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        CryptographicOperations.ZeroMemory(hash);
        return new PasswordAccountRecord(playerId, encoded);
    }

    public string? Authenticate(string credential)
    {
        if (credential is null || credential.Length > 4096) return null;
        var split = credential.IndexOf('\n');
        if (split <= 0 || split != credential.LastIndexOf('\n')) return null;
        var id = credential[..split];
        var password = credential[(split + 1)..];
        if (!ValidId(id) || password.Length is < 1 or > 256 ||
            password.Any(char.IsControl))
            return null;

        var known = _accounts.TryGetValue(id, out var record);
        var expected = known ? record.Hash : _zeroHash;
        var salt = known ? record.Salt : _dummySalt;
        var candidate = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
        var match = CryptographicOperations.FixedTimeEquals(candidate, expected);
        CryptographicOperations.ZeroMemory(candidate);
        // Unknown accounts undergo the same PBKDF2 work, reducing easy enumeration.
        // Rate limiting and capacity must be enforced at an eventual trusted ingress.
        return known && match ? id : null;
    }

    private static (byte[] Salt, byte[] Hash) ParseVerifier(string encoded)
    {
        if (encoded is null) throw new ArgumentException("Missing credential verifier.");
        var parts = encoded.Split('$');
        if (parts.Length != 5 || parts[0] != "v1" || parts[1] != "pbkdf2-sha256" ||
            parts[2] != Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture))
            throw new ArgumentException("Unsupported verifier format or cost.");
        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var hash = Convert.FromBase64String(parts[4]);
            if (salt.Length == SaltBytes && hash.Length == HashBytes)
                return (salt, hash);
        }
        catch (FormatException) { }
        throw new ArgumentException("Invalid credential verifier.");
    }

    private static bool ValidId(string? id) =>
        id is { Length: >= 3 and <= 64 } &&
        id.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-');

    private static void ValidateId(string id)
    {
        if (!ValidId(id))
            throw new ArgumentException("Invalid account ID.");
    }
}
