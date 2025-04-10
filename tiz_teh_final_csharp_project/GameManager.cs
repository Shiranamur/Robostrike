namespace tiz_teh_final_csharp_project;

public class GameManager
{
    private readonly Dictionary<string, Game> _activeGames = new Dictionary<string, Game>();
    private readonly QueueManager _queueManager;
    
    private readonly MapHelper _mapHelper;
    private readonly int MinimumPlayers = 2;

    public GameManager(QueueManager queueManager, MapHelper mapHelper)
    {
        _queueManager = queueManager;
        _mapHelper = mapHelper;
        _queueManager.OnQueueUpdated += OnQueueUpdatedHandler;
    }

    private void OnQueueUpdatedHandler()
    {
        // Call the async Task method and log exceptions.
        if (_queueManager.GetQueuedPlayerIds().Count >= MinimumPlayers)
        {
            _ = GameCreationAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    // Log the exception or handle accordingly.
                    Console.WriteLine($"Error in TryStartGameAsync: {task.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
    public async Task<object> GameCreationAsync()
    
    {
        Console.WriteLine("[debug] Creating new game");
        var queuedIds = _queueManager.GetQueuedPlayerIds().Take(MinimumPlayers).ToList();
        List<Player> players = queuedIds.Select(id => new Player { id = id }).ToList();
        var mapFile = _mapHelper.PickRandomMap();
        var matchId = Guid.NewGuid().ToString();
        var game = new Game(mapFile, players);
        game.MatchId = matchId;
        _activeGames.Add(matchId, game);
        
        // Add players to pending notifications BEFORE removing them from queue
        foreach (var playerId in queuedIds)
        {
            _queueManager.AddToPendingNotifications(playerId);
        }
        
        _queueManager.RemovePlayers(queuedIds);
        _ = Task.Run(() => game.StartGameAsync());

        await NotifyPlayersAsync(queuedIds, matchId);
        
        // todo : not sure if this still usefull with how the longpolling is done for the queues
        return new { message = "Game created", matchId = matchId };
    }
    
    private async Task NotifyPlayersAsync(List<int> playerIds, string matchId)
    {
        // Store which players have been notified
        HashSet<int> notifiedPlayers = new HashSet<int>();
        int maxRetries = 10;
    
        for (int retry = 0; retry < maxRetries; retry++)
        {
            // Try to notify any players who haven't been notified yet
            foreach (var playerId in playerIds.Where(id => !notifiedPlayers.Contains(id)))
            {
                if (_queueManager.UpdateStatusIfCallbackExists(playerId, new Dictionary<string, string> {["game_id"] = matchId }))
                {
                    notifiedPlayers.Add(playerId);
                    Console.WriteLine($"[debug] Player {playerId} notified about game {matchId}");
                }
            }
        
            // If all players notified, break out of the loop
            if (notifiedPlayers.Count == playerIds.Count)
                break;
            
            // Wait a bit before retrying
            await Task.Delay(1000);
        }
    
        // Log any players who couldn't be notified
        foreach (var playerId in playerIds.Where(id => !notifiedPlayers.Contains(id)))
        {
            Console.WriteLine($"[warning] Failed to notify player {playerId} about game {matchId}");
        }
    }    

    public Game GetGame(string matchId)
    {
        _activeGames.TryGetValue(matchId, out var game);
        return game;
    }
}