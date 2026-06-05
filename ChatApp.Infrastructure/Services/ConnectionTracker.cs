using System.Collections.Concurrent;
using ChatApp.Core.Interfaces;

namespace ChatApp.Infrastructure.Services;

public class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, string> _connectionUsers = new();

    public void AddConnection(string userId, string connectionId)
    {
        _connectionUsers[connectionId] = userId;
        _userConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> { connectionId },
            (_, set) =>
            {
                lock (set) { set.Add(connectionId); }
                return set;
            });
    }

    public void RemoveConnection(string connectionId)
    {
        if (!_connectionUsers.TryRemove(connectionId, out var userId))
        {
            return;
        }

        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }
    }

    public IReadOnlyList<string> GetConnections(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }

        return Array.Empty<string>();
    }

    public string? GetUserId(string connectionId)
    {
        return _connectionUsers.TryGetValue(connectionId, out var userId) ? userId : null;
    }
}
