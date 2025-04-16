using System.Collections.Concurrent;

namespace tiz_teh_final_csharp_project;

public class GameManager
{
    private readonly Dictionary<string, Game> _activeGames = new Dictionary<string, Game>();
    private readonly ConcurrentDictionary<int, List<Action<object>>> _statusCallbacks = new();
    private readonly HashSet<int> _pendingNotifications = new HashSet<int>();

    private readonly QueueManager _queueManager;
    private readonly MapHelper _mapHelper;
    private readonly int _minimumPlayers = 2;

    public GameManager(QueueManager queueManager, MapHelper mapHelper)
    {
        _queueManager = queueManager;
        _mapHelper = mapHelper;
        _queueManager.OnQueueUpdated += OnQueueUpdatedHandler;
    }

    private void OnQueueUpdatedHandler()
    {
        // Call the async Task method and log exceptions.
        if (_queueManager.GetQueuedPlayerIds().Count >= _minimumPlayers)
        {
            List<int> playersId = _queueManager.GetQueuedPlayerIds().Take(2).ToList(); 
            _ = GameCreationAsync(playersId).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    // Log the exception or handle accordingly.
                    Console.WriteLine($"Error in TryStartGameAsync: {task.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
    public async Task GameCreationAsync(List<int> playersIds)
    
    {
        Console.WriteLine("[debug] Creating new game");
        List<Player> players = playersIds.Select(id => new Player { Id = id }).ToList();
        var mapFile = _mapHelper.PickRandomMap();
        var matchId = Guid.NewGuid().ToString();
        var game = new Game(mapFile, players, matchId);
        _activeGames.Add(matchId, game);
        
        // Add players to pending notifications BEFORE removing them from queue
        foreach (var playerId in playersIds)
        {
            this.AddToPendingNotifications(playerId);
        }
        
        _queueManager.RemovePlayers(playersIds);
        var startGameTask = Task.Run(() => game.StartGameAsync());

        startGameTask.ContinueWith(async t =>
        {
            if (t.Result == true)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
                lock (_activeGames)
                {
                    _activeGames.Remove(matchId);
                }

                Console.WriteLine($"Game {matchId} finished and was removed from _activeGames.");
            }
        });

        await NotifyPlayersAsync(playersIds, game);
    }
    
    private async Task NotifyPlayersAsync(List<int> playerIds, object game)
    {
        // Store which players have been notified
        HashSet<int> notifiedPlayers = new HashSet<int>();
        int maxRetries = 10;
    
        for (int retry = 0; retry < maxRetries; retry++)
        {
            // Try to notify any players who haven't been notified yet
            playerIds.Where(id => !notifiedPlayers.Contains(id))
                .Where(id => this.UpdateStatusIfCallbackExists(id, game))
                .ToList()  // Materialize the query before applying side effects
                .ForEach(id => 
                {
                    notifiedPlayers.Add(id);
                    Console.WriteLine($"[debug] Player {id} notified about game {game}");
                });
        
            // If all players notified, break out of the loop
            if (notifiedPlayers.Count == playerIds.Count)
                break;
            
            // Wait a bit before retrying
            await Task.Delay(1000);
        }
    
        // Log any players who couldn't be notified
        foreach (var playerId in playerIds.Where(id => !notifiedPlayers.Contains(id)))
        {
            Console.WriteLine($"[warning] Failed to notify player {playerId} about game {game}");
        }
    }    

    public Game? GetGame(string matchId)
    {
        _activeGames.TryGetValue(matchId, out var game);
        return game;
    }
    
    // Registers a callback for the specific player.
    // The callback will be invoked later with a dictionary such as { "game_id": gameId }.
    public void OnStatusUpdate(int playerId, Action<object> callback)
    {
        _statusCallbacks.AddOrUpdate(
            playerId,
            new List<Action<object>>() { callback },
            (_, existingCallbacks) =>
            {
                existingCallbacks.Add(callback);
                return existingCallbacks;
            });
    }
    
    public bool UpdateStatusIfCallbackExists(int playerId, object status)
    {
        if (_statusCallbacks.TryRemove(playerId, out List<Action<object>> callbacks))
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

    public bool IsInPendingNotifications(int playerId)
    {
        return _pendingNotifications.Contains(playerId);
    }

    public string? GetPlayerGameId(int playerId)
    {
        return _activeGames
            .Where(kvp => kvp.Value.Players.Any(p => p.Id == playerId))
            .Select(kvp => kvp.Key)
            .FirstOrDefault();
    }
}