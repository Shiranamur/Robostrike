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
    }

    public async Task<object> WaitForGameCreationAsync()
    {
        while (_queueManager.GetQueuedPlayerIds().Count < MinimumPlayers)
        {
            await Task.Delay(500);
        }
        var queuedIds = _queueManager.GetQueuedPlayerIds().Take(MinimumPlayers).ToList();
        List<Player> players = queuedIds.Select(id => new Player { id = id }).ToList();
        var mapFile = _mapHelper.PickRandomMap();
        var matchId = Guid.NewGuid().ToString();
        var game = new Game(mapFile, players);
        game.MatchId = matchId;
        _activeGames.Add(matchId, game);
        _queueManager.RemovePlayers(queuedIds);
        _ = Task.Run(() => game.StartGameAsync());
        return new { message = "Game created", matchId = matchId };
    }

    public Game GetGame(string matchId)
    {
        _activeGames.TryGetValue(matchId, out var game);
        return game;
    }
}