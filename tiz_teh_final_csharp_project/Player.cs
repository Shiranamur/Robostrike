using System;
using System.Text.Json.Serialization;

namespace tiz_teh_final_csharp_project
{
    public class Player
    {
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public char direction { get; set;}
        public char curInput { get; set; }
        public string inputs { get; set;}
        [JsonPropertyName("previousX")]
        public int xA { get; set; }
        [JsonPropertyName("previousY")]
        public int yA { get; set; }
        public int events { get; set; }
        public int push = 0;
        public char pushDirection;

        public void MoveForward(Map carte)
        {
            int newX = x;
            int newY = y;
            push = 1;
            if (carte == null)
            {
                Console.WriteLine($"Player {id}: Carte is null!");
                return;
            }
            pushDirection = direction;
            if (direction == 'N')
            {
                pushDirection = direction;
                newY += -1;
            }
            else if (direction == 'S')
            {
                pushDirection = direction;
                newY += 1;
            }
            else if (direction == 'E')
            {
                pushDirection = direction;
                newX += 1;
            }
            else if (direction == 'W')
            {
                pushDirection = direction;
                newX += -1;
            }
            else
            {
                Console.WriteLine("Invalid direction");
            }

            bool isValid = carte.isValidMove(newX, newY);
            Console.WriteLine($"Player {id}: Attempting to move to ({newX}, {newY}). Is valid move? {isValid}");

            if (isValid)
            {
                xA = x;
                yA = y;
                x = newX;
                y = newY;
                Console.WriteLine($"Player {id}: Successfully moved pos forward to ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"Player {id}: Move to ({newX}, {newY}) is invalid.");
            }
        }

        public void RotateLeft()
        {
            push = 0;
            Console.WriteLine("Rotating Left");
            if (direction == 'N')
            {
                direction = 'W';
            }
            else if (direction == 'E')
            {
                direction = 'N';
            }
            else if (direction == 'S')
            {
                direction = 'E';
            }
            else if (direction == 'W')
            {
                direction = 'S';
            }
            else
            {
                Console.WriteLine("Direction invalide");
            }
            pushDirection = direction;
        }

        public void RotateRight()
        {
            push = 0;
            Console.WriteLine("Rotating right");
            if (direction == 'N')
            {
                direction = 'E';
            }
            else if (direction == 'E')
            {
                direction = 'S';
            }
            else if (direction == 'S')
            {
                direction = 'W';
            }
            else if (direction == 'W')
            {
                direction = 'N';
            }
            else
            {
                Console.WriteLine("Direction invalide");
            }
            pushDirection = direction;
        }

        public void MoveBackward(Map carte)
        {
            int newX = x;
            int newY = y;
            push = 1;

            if (carte == null)
            {
                Console.WriteLine($"Player {id}: Carte is null!");
                return;
            }
            if (direction == 'N')
            {
                newY += 1;
                pushDirection = 'S';
            }
            else if (direction == 'S')
            {
                newY += -1;
                pushDirection = 'N';
            }
            else if (direction == 'E')
            {
                newX += -1;
                pushDirection = 'W';
            }
            else if (direction == 'W')
            {
                newX += 1;
                pushDirection = 'E';
            }
            else
            {
                Console.WriteLine("Invalid direction");
            }

            bool isValid = carte.isValidMove(newX, newY);
            Console.WriteLine($"Player {id}: Attempting to move to ({newX}, {newY}). Is valid move? {isValid}");

            if (isValid)
            {
                xA = x;
                yA = y;
                x = newX;
                y = newY;
                Console.WriteLine($"Player {id}: Successfully moved backwards to ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"Player {id}: Move to ({newX}, {newY}) is invalid.");
            }
        }

        public string EnterInput(Player player)
        {
            Console.WriteLine("Enter inputs for player " + player.id);
            return (Console.ReadLine());

        }
        //checks number of players at given coordinates
        public int CheckTile(int x, int y, List<Player> Players)
        {
            int i = 0;
            foreach(var player in Players)
            {
                if (player.x == x && player.y == y)
                {
                    i += 1;
                }
            }
            return i;
        }

    public int HandleCollision(Player player1, Player player2, Map carte, List<Player> players)
        {
            if (player1.push > player2.push && player2.push == 0)
            {
                if (player1.pushDirection == 'N')
                {
                    bool isValid = carte.isValidMove(player2.x, player2.y - 1);
                    if (isValid)
                    {
                        player2.y += -1;
                        return 1;
                    }
                    else
                    {
                        player1.x = player1.xA;
                        player1.y = player1.yA;
                        return 0;
                    }
                }
                else if (player1.pushDirection == 'E')
                {
                    player2.push = 1;
                    player2.pushDirection = 'E';
                    bool isValid = carte.isValidMove(player2.x + 1, player2.y);
                    if (isValid && CheckTile(player2.x + 1, player2.y, players) == 1)
                    {
                        Console.WriteLine("prout");
                        foreach(var player in players)
                        {

                            if (player2.x + 1 == player.x && player2.y == player.y)
                            {
                                player.xA = player.x;
                                player.yA = player.y;
                                player2.x += 1;
                                if (HandleCollision(player2, player, carte, players) == 1)
                                {
                                    return 1;
                                }
                                else
                                {
                                    player1.x = player1.xA;
                                    player1.y = player1.yA;
                                    player2.x = player2.xA;
                                    player2.y = player2.yA;
                                    player.x = player.xA;
                                    player.y = player.yA;
                                    return 0;                                 
                                }

                            }
                        }
                    }
                    else if (isValid)
                    {
                        player2.x += 1;
                        return 1;
                    }
                    else
                    {
                        player1.x = player1.xA;
                        player1.y = player1.yA;
                        return 0;
                    }
                }
                else if (player1.pushDirection == 'W')
                {
                    bool isValid = carte.isValidMove(player2.x - 1, player2.y);
                    if (isValid)
                    {
                        player2.x += -1;
                        return 1;
                    }
                    else
                    {
                        player1.x = player1.xA;
                        player1.y = player1.yA;
                        return 0;
                    }
                }
                else if (player1.pushDirection == 'S')
                {
                    bool isValid = carte.isValidMove(player2.x, player2.y + 1);
                    if (isValid)
                    {
                        player2.y += 1;
                        return 1;
                    }
                    else
                    {
                        player1.x = player1.xA;
                        player1.y = player1.yA;
                        return 0;
                    }
                }
                Console.WriteLine("Pushed once");

            }
            else if (player1.push == player2.push)
            {

                player1.x = player1.xA;
                player1.y = player1.yA;
                player2.x = player2.xA;
                player2.y = player2.yA;
                return 0;
            }
            return -1;
        }
    }
}