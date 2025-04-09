using System.Collections.Concurrent;

namespace tiz_teh_final_csharp_project;

public class QueueManager
{
    private readonly List<int> _playerQueue = new List<int>();
    private readonly ConcurrentDictionary<int, List<Action<Dictionary<string, string>>>> _statusCallbacks = new();
    private readonly HashSet<int> _pendingNotifications = new HashSet<int>();

    public delegate void QueueUpdateHandler();

    public event QueueUpdateHandler OnQueueUpdated; // to create an event each time a user is added onto the queue

    /// <summary>
    /// Retourne valeur True si le joueur a déjà était ajouté.
    /// Retourne valeur False si le joueur est déjà dans la liste
    /// </summary>
    /// <param name="playerId">Int userid in db</param>
    /// <returns></returns>
    public bool EnqueuePlayer(int playerId)
    {
        if (_playerQueue.Contains(playerId))
        {
            return false;
        }
        _playerQueue.Add(playerId);
        OnQueueUpdated?.Invoke();
        return true;
    }

    public bool IsPlayerActive(int playerId)
    {
        return _playerQueue.Contains(playerId) || _statusCallbacks.ContainsKey(playerId) ||
               _pendingNotifications.Contains(playerId);
    }

    public bool DequeuePlayer(int playerId)
    {
        if (!_playerQueue.Contains(playerId))
        {
            return false;
        }

        _playerQueue.Remove(playerId);
        return true;
    }

    public List<int> GetQueuedPlayerIds() => _playerQueue.ToList();

    public void RemovePlayers(List<int> players)
    {
        foreach (var id in players)
        {
            _playerQueue.Remove(id);
        }
    }

    // Registers a callback for the specific player.
    // The callback will be invoked later with a dictionary such as { "game_id": gameId }.
    public void OnStatusUpdate(int playerId, Action<Dictionary<string, string>> callback)
    {
        _statusCallbacks.AddOrUpdate(
            playerId,
            new List<Action<Dictionary<string, string>>>() { callback },
            (_, existingCallbacks) =>
            {
                existingCallbacks.Add(callback);
                return existingCallbacks;
            });
    }

    // updates status for the player and fires only the concerned callbacks
    public void UpdateStatus(int playerId, Dictionary<string, string> status)
    {
        // Add to pending notifications before removing from queue
        _pendingNotifications.Add(playerId);

        if (_statusCallbacks.TryRemove(playerId, out List<Action<Dictionary<string, string>>> callbacks))
        {
            foreach (var callback in callbacks)
            {
                callback.Invoke(status);
            }

            // Remove from pending notifications when callback is executed
            _pendingNotifications.Remove(playerId);
        }
    }
    
    public bool UpdateStatusIfCallbackExists(int playerId, Dictionary<string, string> status)
    {
        if (_statusCallbacks.TryRemove(playerId, out List<Action<Dictionary<string, string>>> callbacks))
        {
            foreach (var callback in callbacks)
            {
                callback.Invoke(status);
            }
            return true;
        }
        return false;
    }
    
    public void AddToPendingNotifications(int playerId)
    {
        _pendingNotifications.Add(playerId);
    }
}