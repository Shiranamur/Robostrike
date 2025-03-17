using System.Text.Json;

namespace tiz_teh_final_csharp_project
{
    public class Game
    {
        public Map map { get; set; }
        public List<Player> Players { get; set; }

        public Game(string mapFile, List<Player> players)
        {
            if (!File.Exists(mapFile))
            {
                Console.WriteLine($"Game: Map file not found at {mapFile}");
                return;
            }
            
            string json = File.ReadAllText(mapFile);
            map = JsonSerializer.Deserialize<Map>(json);

            if (map != null)
            {
                Console.WriteLine($"Map initialized with Width: {map.Width}, Height: {map.Height}, Tiles: {map.tiles.Count}");
            }
            else
            {
                Console.WriteLine("Failed to deserialize map.");
            }

            
            if (players == null)
            {
                Console.WriteLine("Game: Players list is null.");
                return;
            }
            
            Players = players;
            
            Console.WriteLine($"Game: Initialized with {Players.Count} players.");
        }

        public void StartGame()
        {
            if (map == null)
            {
                Console.WriteLine("Game: Map is null in startGame.");
                return;
            }
            if (Players == null)
            {
                Console.WriteLine("Game: Players list is null in startGame.");
                return;
            }

            GameLoop();

        }
        public void GameLoop()
        {
            map.printMap(Players);
            foreach (var player in Players)
            {
                player.xA = player.x;
                player.yA = player.y;
                if (player == null)
                {
                    Console.WriteLine("Game: A player in the list is null.");
                    continue;
                }
                player.inputs = player.EnterInput(player);
            }
            for (int j = 0; j < 6; j++)
            {
                foreach (var player in Players)
                {
                    player.ReadInput(player.inputs[j], map);  
                }
                foreach(var qPlayer in Players)
                {
                    foreach(var wPlayer in Players)
                    {
                        if (wPlayer.x == qPlayer.x && wPlayer.y == qPlayer.y && wPlayer.id != qPlayer.id)
                        {
                            qPlayer.HandleCollision(wPlayer,qPlayer, map);
                        }
                    }
                }

             
                map.printMap(Players);
                Console.WriteLine("enter anything to continue");
                Console.ReadLine();
            }
        }
    }
}
