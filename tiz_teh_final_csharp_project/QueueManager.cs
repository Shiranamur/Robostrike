namespace tiz_teh_final_csharp_project;

public class QueueManager
{
    private readonly List<Player> _playerQueue = new List<Player>();

    public void EnqueuePlayer(Player player)
    {
        _playerQueue.Add(player);
    }
    
    public List<Player> GetPlayerQueue() => _playerQueue.ToList();

    public void RemovePlayers(List<Player> players)
    {
        foreach (var player in players)
        {
            _playerQueue.Remove(player);
        }
    }
    

}