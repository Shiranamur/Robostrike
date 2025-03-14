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

        public void MoveForward(Map carte)
        {
            int newX = x;
            int newY = y;
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
        }
        public void RotateRight()
        {
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
        }

        public void MoveBackward(Map carte)
        {
            int newX = x;
            int newY = y;
            if (carte == null)
            {
                Console.WriteLine($"Player {id}: Carte is null!");
                return;
            }
            if (direction == 'N')
            {
                newY += 1;
            }
            else if (direction == 'S')
            {
                newY += -1;
            }
            else if (direction == 'E')
            {
                newX += -1;
            }
            else if (direction == 'W')
            {
                newX += 1;
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
    }

}