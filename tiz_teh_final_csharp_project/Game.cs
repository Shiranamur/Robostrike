using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.InteropServices.Swift;

namespace tiz_teh_final_csharp_project
{
    public class Turn
    {
        public int turnNumber { get; set; }
        public List<Player> players { get; set; }
        public Turn(int turnNumber, List<Player> players)
        {
            this.turnNumber = turnNumber;
            this.players = players;
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
        public void ReadInput(Player player, char i, Map carte)
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
            else
            {
                    Console.WriteLine("Invalid input");
            }
            
        }
        public void GameLoop()
        {
            Turn[] turnData = new Turn[6];
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
                Turn curTurn = new Turn(j, Players);

                foreach (var player in Players)
                {
                    ReadInput(player, player.inputs[j], map);
                    player.curInput = player.inputs[j];
                }
                foreach(var qPlayer in Players)
                {
                    foreach(var wPlayer in Players)
                    {
                        if ((wPlayer.x == qPlayer.x || wPlayer.x == qPlayer.xA) && (wPlayer.y == qPlayer.y || wPlayer.y == qPlayer.yA) && wPlayer.id != qPlayer.id)
                        {
                            qPlayer.HandleCollision(wPlayer,qPlayer, map);
                        }
                    }
                }
                foreach (var player in Players)
                {
                    curTurn.players.Append(player);
                    Console.WriteLine(curTurn.players);
                }
                turnData.Append(curTurn);


             
                map.printMap(Players);
                Console.WriteLine("enter anything to continue");
                Console.ReadLine();
            }
            string test = JsonSerializer.Serialize<Turn[]>(turnData);
            Console.WriteLine(test);
        }
    }
}
