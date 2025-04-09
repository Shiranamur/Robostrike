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

    public async Task<Object> WaitForGameCreationAsync()
    {

        while (_queueManager.GetPlayerQueue().Count < MinimumPlayers)
        {
            await Task.Delay(500);
        }
        
        var players = _queueManager.GetPlayerQueue().Take(MinimumPlayers).ToList();
        var mapFile = _mapHelper.PickRandomMap();
        // wtf is a Guid.NewGuide
        var matchId = Guid.NewGuid().ToString();
        var game = new Game(mapFile, players) {MatchId=matchId};
        _activeGames.Add(matchId, game);
        _queueManager.RemovePlayers(players);
        _ = Task.Run(( )=> game.StartGame());
        
        Console.WriteLine($"Game created: {matchId}");
        return new { message = "game created", matchId = matchId };
    }

    public Game GetGame(string matchId)
    {
        _activeGames.TryGetValue(matchId, out var game);
        return game;
    }
}