using System.Text.Json;

namespace tiz_teh_final_csharp_project
{
    public class Game
    {
        public Map Map { get; set; }
        public List<Player> Players { get; set; }

        public Game(string mapFile, List<Player> players)
        {
            if (!File.Exists(mapFile))
            {
                Console.WriteLine($"Game: Map file not found at {mapFile}");
                return;
            }
            
            string json = File.ReadAllText(mapFile);
            Map = JsonSerializer.Deserialize<Map>(json);
            
            if (Map == null)
            {
                Console.WriteLine("Game: Failed to deserialize map.");
                return;
            }
            
            if (players == null)
            {
                Console.WriteLine("Game: Players list is null.");
                return;
            }
            
            Players = players;
            
            Console.WriteLine($"Game: Initialized with {Players.Count} players.");
        }

        public void startGame()
        {
            if (Map == null)
            {
                Console.WriteLine("Game: Map is null in startGame.");
                return;
            }
            if (Players == null)
            {
                Console.WriteLine("Game: Players list is null in startGame.");
                return;
            }

            foreach (var player in Players)
            {
                if (player == null)
                {
                    Console.WriteLine("Game: A player in the list is null.");
                    continue;
                }
                
                player.Move(player.x + 1, player.y, Map);
                Console.WriteLine($"Player {player.id} moved to ({player.x}, {player.y})");
            }
        }
    }
}
