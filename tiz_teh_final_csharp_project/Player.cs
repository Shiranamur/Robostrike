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
        public int XOld { get; set; }
        public int YOld { get; set; }
        public int Push;
        private char _pushDirection;
        public bool IsAlive = true;
        public int health { get; set; }
        public int hit { get; set; }
        public Dictionary<int, Dictionary<int, int>> ShotHitPlayer = new Dictionary<int, Dictionary<int, int>>();
        public string CollisionType { get; set; }
        public Dictionary<int, int> CollisionCoordinates = new Dictionary<int, int>();
        public int CollisionWithId  { get; set;}
        
        public void MoveForward(Map carte)
        {
            int newX = X;
            int newY = Y;
            Push = 1;
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
            Push = 0;
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
            Push = 0;
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
            Push = 1;

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


        public Dictionary<int, Dictionary<int, int>> Shoot(Map map, List<Player> players)
        {
            // Define direction vectors
            int dx = 0, dy = 0;
            switch (Direction)
            {
                case 'N': dy = -1; break;
                case 'S': dy = 1; break;
                case 'E': dx = 1; break;
                case 'W': dx = -1; break;
            }
    
            // Start from current position and move in the direction
            int checkX = X, checkY = Y;
    
            // Create empty result dictionary
            var result = new Dictionary<int, Dictionary<int, int>>();

            while (true)
            {
                checkX += dx;
                checkY += dy;
                
                // check if we're still within the map
                if (checkX < 0 || checkY < 0 || checkX >= map.Width || checkY >= map.Height)
                    break; // Out of bounds
                
                // Check if any player is hit
                foreach (var player in players)
                {
                    if (player.X == checkX && player.Y == checkY)
                    {
                        player.health -= 1;
                        player.hit += 1;
                        
                        result[player.Id] = new Dictionary<int, int>{{ checkX, checkY }};
                        return result;
                    }
                }
            }

            return result; // no one hit
        }


        public int HandleCollision(Player player1, Player player2, Map map, List<Player> players)
        {

            // handle equal push case first
            if (player1.Push == player2.Push)
            {
                player1.X = player1.XOld;
                player1.Y = player1.YOld;
                player2.X = player2.XOld;
                player2.Y = player2.YOld;
                return 0;
            }
            
            // if player1 doesn't have push priority, no collision handling occurs
            if (!(player1.Push > player2.Push && player2.Push == 0))
            {
                return -1;
            }

            int targetX = player2.X;
            int targetY = player2.Y;

            switch (player1._pushDirection)
            {
                case 'N': targetY -= 1; break;
                case 'S': targetY += 1; break;
                case 'E': targetX += 1; break;
                case 'W': targetX -= 1; break;
            }

            // set player2 push properties
            player2.Push = 1;
            player2._pushDirection = player1._pushDirection;
            
            // check if move is valid
            bool isValid = map.IsValidMove(targetX, targetY);
            
            // invalid move - return player1 to original position
            if (!isValid)
            {
                player1.X = player1.XOld;
                player1.Y = player1.YOld;
                return 0;
            }
            
            // Check if there's another player at the target position
            int tileStatus = CheckTile(targetX, targetY, players);
            
            // empty tile - move player2 to target position
            if (tileStatus == 0)
            {
                player2.X = targetX;
                player2.Y = targetY;
                return 1;
            }

            if (tileStatus == 1)
            {
                foreach (var player in players)
                {
                    if (player.X == targetX && player.Y == targetY)
                    {
                        // Save original positions
                        player.XOld = player.X;
                        player.YOld = player.Y;
                        player2.XOld = player2.X;
                        player2.YOld = player2.Y;

                        // Move player2 to target position
                        player2.X = targetX;
                        player2.Y = targetY;

                        // recursively handle collision with next player
                        if (HandleCollision(player2, player, map, players) == 1)
                        {
                            return 1; // Success
                        }
                        else
                        {
                            // Revert positions if collision chain failed
                            player1.X = player1.XOld;
                            player1.Y = player1.YOld;
                            player2.X = player2.XOld;
                            player2.Y = player2.YOld;
                            player.X = player.XOld;
                            player.Y = player.YOld;
                            return 0; // Failure
                        }
                    }
                }
            }

            return -1;
        }
    }
}