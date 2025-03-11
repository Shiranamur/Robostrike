namespace tiz_teh_final_csharp_project;

public class Program
{
    static void Main(string[] args)
    {
        string mapFilePath = "/home/shiranamur/Documents/Cours/c#/tiz_teh_final_csharp_project/tiz_teh_final_csharp_project/Map/map_test.json";
        
        List<Player> players = new List<Player>
        {
            new Player { id = 1, x = 0, y = 0},
            new Player { id = 2, x = 9, y = 9}
        };
        
        Game game = new Game(mapFilePath, players);
        
        game.startGame();
    }
}

