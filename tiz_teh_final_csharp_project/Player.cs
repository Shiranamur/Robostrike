using System.Text.Json.Serialization;

namespace tiz_teh_final_csharp_project
{
    public class Player
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public char Direction { get; set; }
        public char CurInput { get; set; }
        public string Inputs { get; set; }
        [JsonPropertyName("previousX")] public int XOld { get; set; }
        [JsonPropertyName("previousY")] public int YOld { get; set; }
        public int Events { get; set; }
        private int _push;
        private char _pushDirection;

        public void MoveForward(Map carte)
        {
            int newX = X;
            int newY = Y;
            _push = 1;
            if (carte == null)
            {
                Console.WriteLine($"Player {Id}: Carte is null!");
                return;
            }

            _pushDirection = Direction;
            if (Direction == 'N')
            {
                newY += -1;
            }
            else if (Direction == 'S')
            {
                newY += 1;
            }
            else if (Direction == 'E')
            {
                newX += 1;
            }
            else if (Direction == 'W')
            {
                newX += -1;
            }
            else
            {
                Console.WriteLine("Invalid direction");
            }

            bool isValid = carte.IsValidMove(newX, newY);
            Console.WriteLine($"Player {Id}: Attempting to move to ({newX}, {newY}). Is valid move? {isValid}");

            if (isValid)
            {
                XOld = X;
                YOld = Y;
                X = newX;
                Y = newY;
                Console.WriteLine($"Player {Id}: Successfully moved pos forward to ({X}, {Y})");
            }
            else
            {
                Console.WriteLine($"Player {Id}: Move to ({newX}, {newY}) is invalid.");
            }
        }

        public void RotateLeft()
        {
            _push = 0;
            Console.WriteLine("Rotating Left");
            if (Direction == 'N')
            {
                Direction = 'W';
            }
            else if (Direction == 'E')
            {
                Direction = 'N';
            }
            else if (Direction == 'S')
            {
                Direction = 'E';
            }
            else if (Direction == 'W')
            {
                Direction = 'S';
            }
            else
            {
                Console.WriteLine("Direction invalide");
            }

            _pushDirection = Direction;
        }

        public void RotateRight()
        {
            _push = 0;
            Console.WriteLine("Rotating right");
            if (Direction == 'N')
            {
                Direction = 'E';
            }
            else if (Direction == 'E')
            {
                Direction = 'S';
            }
            else if (Direction == 'S')
            {
                Direction = 'W';
            }
            else if (Direction == 'W')
            {
                Direction = 'N';
            }
            else
            {
                Console.WriteLine("Direction invalide");
            }

            _pushDirection = Direction;
        }

        public void MoveBackward(Map carte)
        {
            int newX = X;
            int newY = Y;
            _push = 1;

            if (carte == null)
            {
                Console.WriteLine($"Player {Id}: Carte is null!");
                return;
            }

            if (Direction == 'N')
            {
                newY += 1;
                _pushDirection = 'S';
            }
            else if (Direction == 'S')
            {
                newY += -1;
                _pushDirection = 'N';
            }
            else if (Direction == 'E')
            {
                newX += -1;
                _pushDirection = 'W';
            }
            else if (Direction == 'W')
            {
                newX += 1;
                _pushDirection = 'E';
            }
            else
            {
                Console.WriteLine("Invalid direction");
            }

            bool isValid = carte.IsValidMove(newX, newY);
            Console.WriteLine($"Player {Id}: Attempting to move to ({newX}, {newY}). Is valid move? {isValid}");

            if (isValid)
            {
                XOld = X;
                YOld = Y;
                X = newX;
                Y = newY;
                Console.WriteLine($"Player {Id}: Successfully moved backwards to ({X}, {Y})");
            }
            else
            {
                Console.WriteLine($"Player {Id}: Move to ({newX}, {newY}) is invalid.");
            }
        }

        public static string? EnterInput(Player player)
        {
            Console.WriteLine("Enter inputs for player " + player.Id);
            return Console.ReadLine();
        }

        //checks number of players at given coordinates
        public static int CheckTile(int x, int y, List<Player> players)
        {
            int i = 0;
            foreach (var player in players)
            {
                if (player.X == x && player.Y == y)
                {
                    i += 1;
                }
            }

            return i;
        }

        public int HandleCollision(Player player1, Player player2, Map carte, List<Player> players)
        {
            if (player1._push > player2._push && player2._push == 0)
            {
                if (player1._pushDirection == 'N')
                {
                    player2._push = 1;
                    player2._pushDirection = 'N';
                    bool isValid = carte.IsValidMove(player2.X, player2.Y - 1);
                    if (isValid && CheckTile(player2.X, player2.Y - 1, players) == 1)
                    {
                        foreach (var player in players)
                        {
                            if (player2.X == player.X && player2.Y - 1 == player.Y)
                            {
                                player.YOld = player.Y;
                                player2.YOld = player2.Y;
                                player2.Y += -1;
                                if (HandleCollision(player2, player, carte, players) == 1)
                                {
                                    Console.WriteLine("1");
                                    return 1;
                                }
                                else
                                {
                                    player1.Y = player1.YOld;
                                    player2.Y = player2.YOld;
                                    player.Y = player.YOld;

                                    return 0;
                                }
                            }
                        }
                    }
                    else if (isValid && CheckTile(player2.X, player2.Y - 1, players) == 0)
                    {
                        player2.Y += -1;
                        return 1;
                    }
                    else
                    {
                        player1.Y = player1.YOld;
                        return 0;
                    }
                }
                else if (player1._pushDirection == 'E')
                {
                    player2._push = 1;
                    player2._pushDirection = 'E';
                    bool isValid = carte.IsValidMove(player2.X + 1, player2.Y);
                    if (isValid && CheckTile(player2.X + 1, player2.Y, players) == 1)
                    {
                        Console.WriteLine("prout");
                        foreach (var player in players)
                        {
                            if (player2.X + 1 == player.X && player2.Y == player.Y)
                            {
                                player.XOld = player.X;
                                player2.XOld = player2.X;
                                player2.X += 1;
                                if (HandleCollision(player2, player, carte, players) == 1)
                                {
                                    Console.WriteLine("1");
                                    return 1;
                                }
                                else
                                {
                                    player1.X = player1.XOld;
                                    player2.X = player2.XOld;
                                    player.X = player.XOld;

                                    return 0;
                                }
                            }
                        }
                    }
                    else if (isValid && CheckTile(player2.X + 1, player2.Y, players) == 0)
                    {
                        player2.X += 1;
                        return 1;
                    }
                    else
                    {
                        player1.X = player1.XOld;
                        return 0;
                    }
                }
                else if (player1._pushDirection == 'W')
                {
                    player2._push = 1;
                    player2._pushDirection = 'W';
                    bool isValid = carte.IsValidMove(player2.X - 1, player2.Y);
                    if (isValid && CheckTile(player2.X - 1, player2.Y, players) == 1)
                    {
                        Console.WriteLine("prout");
                        foreach (var player in players)
                        {
                            if (player2.X - 1 == player.X && player2.Y == player.Y)
                            {
                                player.XOld = player.X;
                                player2.XOld = player2.X;
                                player2.X += -1;
                                if (HandleCollision(player2, player, carte, players) == 1)
                                {
                                    Console.WriteLine("1");
                                    return 1;
                                }
                                else
                                {
                                    player1.X = player1.XOld;
                                    player2.X = player2.XOld;
                                    player.X = player.XOld;

                                    return 0;
                                }
                            }
                        }
                    }
                    else if (isValid && CheckTile(player2.X - 1, player2.Y, players) == 0)
                    {
                        player2.X += -1;
                        return 1;
                    }
                    else
                    {
                        player1.X = player1.XOld;
                        return 0;
                    }
                }
                else if (player1._pushDirection == 'S')
                {
                    player2._push = 1;
                    player2._pushDirection = 'S';
                    bool isValid = carte.IsValidMove(player2.X, player2.Y + 1);
                    if (isValid && CheckTile(player2.X, player2.Y + 1, players) == 1)
                    {
                        Console.WriteLine("prout");
                        foreach (var player in players)
                        {
                            if (player2.X == player.X && player2.Y + 1 == player.Y)
                            {
                                player.YOld = player.Y;
                                player2.YOld = player2.Y;
                                player2.Y += 1;
                                if (HandleCollision(player2, player, carte, players) == 1)
                                {
                                    Console.WriteLine("1");
                                    return 1;
                                }
                                else
                                {
                                    player1.Y = player1.YOld;
                                    player2.Y = player2.YOld;
                                    player.Y = player.YOld;

                                    return 0;
                                }
                            }
                        }
                    }
                    else if (isValid && CheckTile(player2.X, player2.Y + 1, players) == 0)
                    {
                        player2.Y += 1;
                        return 1;
                    }
                    else
                    {
                        player1.Y = player1.YOld;
                        return 0;
                    }
                }
                else if (player1._push == player2._push)
                {
                    player1.X = player1.XOld;
                    player1.Y = player1.YOld;
                    player2.X = player2.XOld;
                    player2.Y = player2.YOld;
                    return 0;
                }
            }

            return -1;
        }
    }
}