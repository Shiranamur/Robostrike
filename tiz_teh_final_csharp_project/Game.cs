using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.InteropServices.Swift;

namespace tiz_teh_final_csharp_project
{
    public class Turn
    {
        public int turnNumber { get; set; }
        public List<Player> Players { get; set; }
        public Turn(int turnNumber, List<Player> players)
        {
            this.turnNumber = turnNumber;
            Players = players;
        }


    }

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
        public void ReadInput(Player player, char i, Map carte, List<Player> Players)
        {
            if (player.health > 0)
            {
                if (i == 'a')
                {
                    player.RotateLeft();
                }
                else if (i == 'z')
                {
                    player.MoveForward(carte);
                }
                else if (i == 's')
                {
                    player.MoveBackward(carte);
                }
                else if (i == 'e')
                {
                    player.RotateRight();
                }
                else if (i == 'd')
                {
                    player.Shoot(carte, Players);
                    player.trigger = 1;
                }
                else
                {
                    Console.WriteLine("Invalid input");
                }
            }
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
            //Turn[5] round;
            for (int j = 0; j < 6; j++)
            {
                foreach (var player in Players)
                {
                    ReadInput(player, player.inputs[j], map, Players);
                }
                foreach (var qPlayer in Players)
                {
                    foreach (var wPlayer in Players)
                    {
                        if (wPlayer.x == qPlayer.x && wPlayer.y == qPlayer.y && wPlayer.id != qPlayer.id)
                        {
                            qPlayer.HandleCollision(wPlayer, qPlayer, map, Players);
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
