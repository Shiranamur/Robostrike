namespace tiz_teh_final_csharp_project;

public class QueueManager
{
    private readonly List<int> _playerQueue = new List<int>();

    public void EnqueuePlayer(int playerId)
    {
        _playerQueue.Add(playerId);
    }
    
    public List<int> GetQueuedPlayerIds() => _playerQueue.ToList();

    public void RemovePlayers(List<int> players)
    {
        foreach (var id in players)
        {
            _playerQueue.Remove(id);
        }
    }
}