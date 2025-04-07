namespace BlazorApp1.Class;

public class Shot : GameEntity
{
    public Player Parent { get; set; }

    public Shot(int x, int y, Player parent, Map map)
    {
        X = x;
        Y = y;
        Parent = parent;
        Map = map;
        Direction = parent.Direction; // Inherit direction from the parent
        Sprite = $"images/Sprites/plasma_player{Parent.Id}.png";
    }
    
    public override bool MoveForward(bool moveForward = true)
    {
        // Custom logic before the base implementation.
        // Call the base method to use its original behavior.
        bool isinbound = base.MoveForward(moveForward);
            
        // Additional behavior after base.MoveForward.
        
        return !isinbound;
    }
}