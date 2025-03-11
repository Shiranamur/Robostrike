namespace tiz_teh_final_csharp_project
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Tile> tiles { get; set; }

        public bool isValidMove(int x, int y)
        {
            if (tiles == null)
            {
                Console.WriteLine("Map: Tiles list is null!");
                return false;
            }
            
            bool isInBounds = x >= 0 && x < Width && y >= 0 && y < Height;
            Console.WriteLine($"Map: Checking move to ({x}, {y}). Is within bounds? {isInBounds}");

            if (!isInBounds)
            {
                return false;
            }
            
            Tile tile = tiles.Find(t => t.X == x && t.Y == y);
            if (tile == null)
            {
                Console.WriteLine($"Map: No tile found at ({x}, {y})");
                return false;
            }

            bool isValid = tile.Type != "obstacle";
            Console.WriteLine($"Map: Tile at ({x}, {y}) is of type {tile.Type}. Is valid move? {isValid}");

            return isValid;
        }
    }
}