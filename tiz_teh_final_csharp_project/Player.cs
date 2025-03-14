using System;

namespace tiz_teh_final_csharp_project
{
    public class Player
    {
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public char direction { get; set;}

        public void Move(int z, Map carte)
        {
            int newX = x;
            int newY = y;
            if (carte == null)
            {
                Console.WriteLine($"Player {id}: Carte is null!");
                return;
            }
            if (direction == 'N' || direction == 'S')
            {
                newX += z;
            }
            else if (direction == 'E' || direction == 'W')
            {
                newY += z;
            }

            bool isValid = carte.isValidMove(newX, newY);
            Console.WriteLine($"Player {id}: Attempting to move to ({newX}, {newY}). Is valid move? {isValid}");

            if (isValid)
            {
                x = newX;
                y = newY;
                Console.WriteLine($"Player {id}: Successfully moved to ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"Player {id}: Move to ({newX}, {newY}) is invalid.");
            }
        }
    }
}