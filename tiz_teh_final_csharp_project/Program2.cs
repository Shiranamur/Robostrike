namespace tiz_teh_final_csharp_project;

public class Program2
{
    static void Main(string[] args)
    {
        string mapFilePath = "../../../Map/map_test.json";
        string currentDir = Directory.GetCurrentDirectory();
        string fullMapFilePath = Path.GetFullPath(mapFilePath);

        Console.WriteLine(currentDir, fullMapFilePath);
        List<Player> players = new List<Player>
        {
            new Player { id = 1, x = 0, y = 0, direction = 'S', inputs = " "},
            new Player { id = 2, x = 2, y = 0, direction = 'N', inputs = " "}
        };
        
        Game game = new Game(mapFilePath, players);
        
        game.StartGame();
    }
}

