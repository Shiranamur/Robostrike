namespace BlazorApp1.Class;

public class Player : GameEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; } = 0;
    public List<Shot> Shots { get; set; } = new List<Shot>();

    // Initialize inherited properties through constructor logic
    public Player(int id, int x, int y, string name, Map map)
    {
        Id = id;
        Name = name;
        X = x;
        Y = y;
        Map = map;
        Direction = Direction.Up;
        Sprite = $"images/Sprites/player{id}.png";
    }

    public void RotateRight(bool rotateRight)
    {
        // calculate new direction based on curent direction and rotation direction
        int directionCount = Enum.GetValues<Direction>().Length;
        int currentDirection = (int)this.Direction;
        
        // for right rotation: add 1 and wrap
        // for left rotation: substract 1 and wrap
        int newDirection = rotateRight ? (currentDirection + 1) % directionCount : (currentDirection - 1 + directionCount) % directionCount;
        
        this.Direction = (Direction)newDirection;
    }
    
    
    public void MoveShots()
    {
        List<Shot> shotsToRemove = new List<Shot>();

        foreach (Shot shot in this.Shots)
        {
            bool shouldRemove = shot.MoveForward(true);
            if (shouldRemove)
            {
                shotsToRemove.Add(shot);
            }
        }

        foreach (Shot shot in shotsToRemove)
        {
            this.Shots.Remove(shot);
        }
    }
}