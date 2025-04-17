namespace BlazorApp1.Class;

public abstract class GameEntity
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Sprite { get; set; }
    public Direction Direction { get; set; }
    public Map Map { get; set; }

    // Common property to generate a CSS class from the direction
    public string DirectionCssClass => $"rotation_{Direction.ToString().ToLowerInvariant()}";
    
    private (int x, int y) GetDirectionOffset(Direction direction)
    {
        // direction matrice to multiply the movement from
        return direction switch
        {
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            _ => (0, 0)
        };
    }
    
    public virtual bool MoveForward(bool moveForward)
    {
        var (dx, dy) = GetDirectionOffset(Direction);
        // 1 forward else -1 backwards
        int multiplier = moveForward ? 1 : -1;
        
        // calculate new positions
        int newX = X + dx * multiplier;
        int newY = Y + dy * multiplier;
        
        // check out of bounds
        if (newX >= 0 && newX < Map.MapWidth && newY >= 0 && newY < Map.MapHeight)
        {
            // assign new positions
            X = newX;
            Y = newY;
            return true;
        }
        // executes only if didn't trigger
        Console.WriteLine("Attempted to move out of bounds, lost a turn ;).");
        return false;
    }

    public void Reposition(int x, int y, Direction direction)
    {
        this.X = x;
        this.Y = y;
        this.Direction = direction;
    }
}