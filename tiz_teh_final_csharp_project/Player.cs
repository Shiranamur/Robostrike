using System;

namespace tiz_teh_final_csharp_project
{
    public class Player
    {
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public char direction { get; set;}
        public string inputs { get; set;}
        public int xA;
        public int yA;
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
            if (direction == 'N')
            {
                newY += -1;
            }
            else if (direction == 'S')
            {
                newY += 1;
            }
            else if (direction == 'E')
            {
                newX += 1;
            }
            else if (direction == 'W')
            {
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
                x = newX;
                y = newY;
                Console.WriteLine($"Player {id}: Successfully moved forward to ({x}, {y})");
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
        public void ReadInput(char i, Map carte)
        {
            if (i == 'q')
            {
                    RotateLeft();
            }
            else if (i == 'w')
            {
                    MoveForward(carte);
            }
            else if (i == 's')
            {
                    MoveBackward(carte);
            }
            else if (i == 'e')
            {
                    RotateRight();
            }
            else
            {
                    Console.WriteLine("Invalid input");
            }
            
        }

        public void HandleCollision(Player player1, Player player2)
        {
            if (player1.push > player2.push && player2.push == 0)
            {
                if (player1.pushDirection == 'N')
                {
                    player2.y += -1;
                }
                else if (player1.pushDirection == 'E')
                {
                    player2.x += 1;
                }
                else if (player1.pushDirection == 'W')
                {
                    player2.x += -1;
                }
                else if (player1.pushDirection == 'S')
                {
                    player2.y += 1;
                }

            }
            else if (player1.push == player2.push)
            {
                player1.x = player1.xA;
                player1.y = player1.yA;
                player2.x = player2.xA;
                player2.y = player2.yA;
            }
        }
    }

}