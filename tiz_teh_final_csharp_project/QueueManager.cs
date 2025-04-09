namespace tiz_teh_final_csharp_project
{
    public class QueueManager
    {
        private readonly List<int> _playerQueue = new List<int>();

        /// <summary>
        /// Ajoute un joueur à la file.
        /// Retourne True si le joueur a bien été ajouté, False s'il est déjà présent.
        /// </summary>
        /// <param name="playerId">Identifiant du joueur</param>
        /// <returns>booléen indiquant le résultat de l'ajout.</returns>
        public bool EnqueuePlayer(int playerId)
        {
            if (_playerQueue.Contains(playerId))
            {
                Console.WriteLine($"Player {playerId} is already in the queue.");
                return false;
            }
            _playerQueue.Add(playerId);
            Console.WriteLine($"Player {playerId} successfully added to the queue.");
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
}