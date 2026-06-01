namespace ChatApp.Core.Interfaces;

public interface IConnectionTracker
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string connectionId);
    IReadOnlyList<string> GetConnections(string userId);
    string? GetUserId(string connectionId);
}
