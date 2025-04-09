namespace tiz_teh_final_csharp_project;

public class QueueManager
{
    private readonly List<int> _playerQueue = new List<int>();

    /// <summary>
    /// Retourne valeur True si le joueur a déjà était ajouté.
    /// Retourne valeur False si le joueur est déjà dans la liste
    /// </summary>
    /// <param name="playerId">Int userid in db</param>
    /// <returns></returns>
    public bool EnqueuePlayer(int playerId)
    {
        if (_playerQueue.Contains(playerId))
        {
            return false;
        }
        _playerQueue.Add(playerId);
        return true;
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