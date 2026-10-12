using System.Security.Cryptography;
using System.Text;
using Arthea.Core.World;

namespace Arthea.Core.Social;

/// <summary>
/// A trusted caller-supplied credential verifier. Implementations must perform real
/// authentication at a reviewed transport boundary; never trust a user-supplied ID.
/// This library supplies NO password, account, or network authenticator.
/// </summary>
public interface IPlayerAuthenticator
{
    string? Authenticate(string credential);
}

/// <summary>The bearer token is sensitive and must never be logged or persisted as plaintext.</summary>
public sealed record SessionHandle(string Token, string PlayerId, DateTimeOffset ExpiresAt);

/// <summary>
/// In-memory, single-process session ownership and short-lived reconnect prototype.
/// One active or suspended session is allowed per identity. All game operations require
/// possession of a nonexpired bearer token derived from the injected authenticator.
/// </summary>
public sealed class RoomSessionCoordinator
{
    private sealed class Session(string playerId, DateTimeOffset expiresAt)
    {
        public string PlayerId { get; } = playerId;
        public DateTimeOffset ExpiresAt { get; set; } = expiresAt;
        public bool Active { get; set; } = true;
    }

    private readonly object _gate = new();
    private readonly RoomChatRuntime _chat;
    private readonly IPlayerAuthenticator _authenticator;
    private readonly TimeProvider _time;
    private readonly TimeSpan _ttl;
    private readonly int _capacity;
    private readonly Dictionary<string, Session> _sessions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _byPlayer = new(StringComparer.Ordinal);

    public RoomSessionCoordinator(RoomChatRuntime chat, IPlayerAuthenticator authenticator,
        TimeProvider? clock = null, TimeSpan? sessionLifetime = null, int maxSessions = 1000)
    {
        _chat = chat ?? throw new ArgumentNullException(nameof(chat));
        _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        _time = clock ?? TimeProvider.System;
        _ttl = sessionLifetime ?? TimeSpan.FromMinutes(15);
        if (_ttl < TimeSpan.FromSeconds(1) || _ttl > TimeSpan.FromHours(24))
            throw new ArgumentOutOfRangeException(nameof(sessionLifetime));
        if (maxSessions is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(maxSessions));
        _capacity = maxSessions;
    }

    /// <summary>
    /// Authenticate outside the state lock, then atomically claim a player. The caller
    /// must supply credentials over a separately authenticated and encrypted transport.
    /// </summary>
    public SessionHandle Connect(string credential, string initialRoom)
    {
        if (string.IsNullOrWhiteSpace(credential) || credential.Length > 4096)
            throw new InvalidOperationException("Authentication failed.");
        var playerId = _authenticator.Authenticate(credential);
        if (string.IsNullOrWhiteSpace(playerId) || playerId.Length > 128 ||
            playerId.Any(char.IsControl))
            throw new InvalidOperationException("Authentication failed.");

        lock (_gate)
        {
            var now = _time.GetUtcNow();
            PruneExpired(now);
            if (_byPlayer.ContainsKey(playerId) || _sessions.Count >= _capacity)
                throw new InvalidOperationException("Session admission denied.");
            var (token, digest) = NewToken();
            var expiry = now + _ttl;
            _chat.Enter(playerId, initialRoom);
            var session = new Session(playerId, expiry);
            _sessions.Add(digest, session);
            _byPlayer.Add(playerId, digest);
            return new SessionHandle(token, playerId, expiry);
        }
    }

    /// <summary>
    /// Resume on another connection with the current bearer token; atomically revoke
    /// that token and rotate to a new one. Old and concurrently replayed tokens fail.
    /// </summary>
    public SessionHandle Resume(string token)
    {
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            PruneExpired(now);
            var (digest, session) = Lookup(token, requireActive: false);
            var (nextToken, nextDigest) = NewToken();
            _sessions.Remove(digest);
            session.Active = true;
            session.ExpiresAt = now + _ttl;
            _sessions.Add(nextDigest, session);
            _byPlayer[session.PlayerId] = nextDigest;
            return new SessionHandle(nextToken, session.PlayerId, session.ExpiresAt);
        }
    }

    /// <summary>Mark the transport disconnected without releasing room membership.</summary>
    public void Suspend(string token)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (_, session) = Lookup(token, requireActive: true);
            session.Active = false;
        }
    }

    public RoomChatMessage Say(string token, string text)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (_, session) = Lookup(token, requireActive: true);
            return _chat.Say(session.PlayerId, text);
        }
    }

    public bool TryMove(string token, Direction direction)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (_, session) = Lookup(token, requireActive: true);
            return _chat.TryMove(session.PlayerId, direction);
        }
    }

    public string CurrentRoom(string token)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (_, session) = Lookup(token, requireActive: true);
            return _chat.CurrentRoom(session.PlayerId);
        }
    }

    public RoomReplay Replay(string token, long afterSequence, int maxCount = 50)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (_, session) = Lookup(token, requireActive: true);
            return _chat.Replay(session.PlayerId, afterSequence, maxCount);
        }
    }

    public void Logout(string token)
    {
        lock (_gate)
        {
            PruneExpired(_time.GetUtcNow());
            var (digest, session) = Lookup(token, requireActive: false);
            RemoveSession(digest, session);
        }
    }

    /// <summary>Explicit cleanup for idle servers without timer or background thread.</summary>
    public int ExpireSessions()
    {
        lock (_gate) return PruneExpired(_time.GetUtcNow());
    }

    private int PruneExpired(DateTimeOffset now)
    {
        var expired = _sessions.Where(pair => pair.Value.ExpiresAt <= now).ToArray();
        foreach (var (digest, session) in expired)
            RemoveSession(digest, session);
        return expired.Length;
    }

    private void RemoveSession(string digest, Session session)
    {
        _sessions.Remove(digest);
        _byPlayer.Remove(session.PlayerId);
        _chat.Leave(session.PlayerId);
    }

    private (string Digest, Session Session) Lookup(string token, bool requireActive)
    {
        // A fixed-size random bearer token is required. No credential details in errors.
        if (token is null || token.Length != 64 ||
            !token.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f'))
            throw new InvalidOperationException("Session unavailable.");
        var digest = Digest(token);
        if (!_sessions.TryGetValue(digest, out var session) || (requireActive && !session.Active))
            throw new InvalidOperationException("Session unavailable.");
        return (digest, session);
    }

    private (string Token, string Digest) NewToken()
    {
        for (var attempt = 0; attempt < 4; attempt++)
        {
            var token = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
            var digest = Digest(token);
            if (!_sessions.ContainsKey(digest))
                return (token, digest);
        }
        throw new InvalidOperationException("Cannot allocate a session token.");
    }

    private static string Digest(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(token)));
}
