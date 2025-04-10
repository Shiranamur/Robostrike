using System.Text.Json;
using System.Collections.Concurrent;

namespace tiz_teh_final_csharp_project
{
    /// <summary>
    /// Représente un tour de jeu.
    /// Appelé avec Turn(int turnNumber, List<Player> players) et retourne un objet Turn contenant le numéro de tour et la liste des joueurs.
    /// </summary>
    public class Turn
    {
        public int TurnNumber { get; set; }
        public List<Player> Players { get; set; }

        public Turn(int turnNumber, List<Player> players)
        {
            this.TurnNumber = turnNumber;
            Players = players;
        }
    }

    /// <summary>
    /// Représente une partie de jeu.
    /// La classe Game charge la carte depuis un fichier JSON, initialise la liste des joueurs (enrichie avec leur position, direction, etc.),
    /// traite les entrées des joueurs de manière asynchrone et gère la boucle de jeu répartie en rounds.
    /// </summary>
    public class Game
    {
        public Map map { get; set; }
        public List<Player> Players { get; set; }
        public string MatchId { get; set; }

        private int currentRound = 0;

        // Stocke les inputs des joueurs pour chaque round.
        // Key: numéro de round ; Value: dictionnaire associant l'id du joueur à son caractère d'entrée.
        private readonly ConcurrentDictionary<int, Dictionary<int, char>> _roundInputs = new();

        // Stocke un TaskCompletionSource pour chaque round afin d'attendre que tous les joueurs aient soumis leur input.
        private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _roundInputCompletionSources = new();

        /// <summary>
        /// Constructeur de la classe Game.
        /// Charge la carte à partir d'un fichier JSON et initialise la liste des joueurs.
        /// Appelé avec Game(string mapFile, List<Player> players) et retourne une instance de Game initialisée.
        /// </summary>
        /// <param name="mapFile">Le chemin du fichier JSON contenant la carte.</param>
        /// <param name="players">La liste minimale des joueurs (avec uniquement leur identifiant, qui sera enrichi).</param>
        public Game(string mapFile, List<Player> players)
        {
            if (!File.Exists(mapFile))
            {
                Console.WriteLine($"Game: Map file not found at {mapFile}");
                return;
            }
            
            string json = File.ReadAllText(mapFile);
            map = JsonSerializer.Deserialize<Map>(json);

            if (map != null)
            {
                Console.WriteLine($"Map initialized with Width: {map.Width}, Height: {map.Height}, Tiles: {map.tiles.Count}");
            }
            else
            {
                Console.WriteLine("Failed to deserialize map.");
            }
            
            if (players == null || players.Count == 0)
            {
                Console.WriteLine("Game: Players list is null or empty.");
                return;
            }
            
            Players = players;
            
            // Initialise les joueurs (définit position, direction et input par défaut)
            InitializePlayers();

            Console.WriteLine($"Game: Initialized with {Players.Count} players.");
        }


        /// <summary>
        /// Initialise les joueurs en leur assignant une position de départ, une direction par défaut et des inputs par défaut.
        /// Méthode de test qui répartit les joueurs en grille sur la carte.
        /// Appelée automatiquement lors de l'initialisation de la partie (void).
        /// </summary>
        private void InitializePlayers()
        {
            int n = Players.Count;
            int gridColumns = (int)Math.Ceiling(Math.Sqrt(n));
            int gridRows = (int)Math.Ceiling((double)n / gridColumns);
            int spacingX = map.Width / (gridColumns + 1);
            int spacingY = map.Height / (gridRows + 1);

            for (int i = 0; i < n; i++)
            {
                int row = i / gridColumns;
                int col = i % gridColumns;

                Players[i].x = spacingX * (col + 1);
                Players[i].y = spacingY * (row + 1);
                Players[i].direction = 'S';  // Direction par défaut (Sud)
                Players[i].inputs = string.Empty; // Les inputs seront reçus via l'API
            }
        }
        
        /// <summary>
        /// Retourne l'état initial de la partie.
        /// Appelée avec GetInitialState() et retourne un objet anonyme contenant le MatchId, la carte et la liste des joueurs avec leurs positions et directions.
        /// réponse à la requête : GET /api/matchmaking/longpoll
        /// </summary>
        /// <returns>Un objet représentant l'état initial du jeu.</returns>
        public object GetInitialState()
        {
            return new 
            {
                matchId = this.MatchId,
                map = this.map,
                players = this.Players.Select(p => new 
                {
                    id = p.id,
                    x = p.x,
                    y = p.y,
                    direction = p.direction,
                    curInput = p.curInput,
                }).ToList()
            };
        }

        /// <summary>
        /// Soumet une entrée de joueur pour un round donné.
        /// Appelée avec SubmitPlayerInput(int playerId, char input) et retourne void.
        /// Elle enregistre l'input du joueur et, si tous les inputs sont reçus, déclenche la complétion de la tâche d'attente.
        /// est trigger par : POST /api/game/{matchId}/round
        /// </summary>
        /// <param name="roundNumber">Le numéro du round en cours.</param>
        /// <param name="playerId">L'identifiant du joueur.</param>
        /// <param name="input">L'entrée saisie par le joueur.</param>
        public void SubmitPlayerInput(int playerId, char input)
        {
            // Récupère ou crée le dictionnaire des inputs pour le round.
            var inputs = _roundInputs.GetOrAdd(currentRound, new Dictionary<int, char>());
            lock (inputs)
            {
                inputs[playerId] = input;
                // Vérifie si les inputs de tous les joueurs ont été reçus.
                if (inputs.Count >= Players.Count)
                {
                    // Signale que la collecte d'inputs est terminée.
                    if (_roundInputCompletionSources.TryGetValue(currentRound, out var tcs))
                    {
                        tcs.TrySetResult(true);
                    }
                }
            }
        }

        /// <summary>
        /// Attend de façon asynchrone que tous les joueurs aient soumis leurs inputs pour un round donné.
        /// Appelée avec WaitForRoundInputsAsync(int roundNumber) et retourne une Task qui se complète quand tous les inputs sont reçus.
        /// </summary>
        /// <param name="roundNumber">Le numéro du round.</param>
        private async Task WaitForRoundInputsAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            _roundInputCompletionSources[currentRound] = tcs;
            
            // Vous pouvez ajouter un timeout ici si nécessaire.
            await tcs.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Traite un round unique en attendant la soumission de tous les inputs puis en appliquant les actions des joueurs.
        /// Appelée avec ProcessRoundAsync(int roundNumber) et retourne une Task.
        /// </summary>
        /// <param name="roundNumber">Le numéro du round à traiter.</param>
        private async Task ProcessRoundAsync()
        {
            // Attend que tous les joueurs aient soumis leur input pour ce round.
            await WaitForRoundInputsAsync();

            // Récupère les inputs pour le round.
            if (!_roundInputs.TryGetValue(currentRound, out var inputs))
            {
                Console.WriteLine($"No inputs received for round {currentRound}");
                return;
            }

            // Traite l'input de chaque joueur.
            foreach (var player in Players)
            {
                if (inputs.TryGetValue(player.id, out char input))
                {
                    // Traite l'input via la fonction ReadInput.
                    ReadInput(player, input, map);
                    player.curInput = input;
                }
                else
                {
                    Console.WriteLine($"Player {player.id} did not submit input for round {currentRound}.");
                }
            }
            
            // Gère les collisions entre joueurs.
            foreach (var qPlayer in Players)
            {
                foreach (var wPlayer in Players)
                {
                    if ((wPlayer.x == qPlayer.x || wPlayer.x == qPlayer.xA) &&
                        (wPlayer.y == qPlayer.y || wPlayer.y == qPlayer.yA) &&
                        wPlayer.id != qPlayer.id)
                    {
                        qPlayer.HandleCollision(wPlayer, qPlayer, map);
                        qPlayer.events = 1;
                        wPlayer.events = 1;
                    }
                }
            }
        }
        
        /// <summary>
        /// Exécute de manière asynchrone la boucle de jeu qui gère plusieurs rounds.
        /// Appelée avec GameLoopAsync() et retourne une Task.
        /// Pour chaque round, elle attend la réception des inputs, traite le round, affiche l'état, puis attend un délai avant de passer au suivant.
        /// </summary>
        public async Task GameLoopAsync()
        {
            for (int j = 0; j < 6; j++)
            {
                await ProcessRoundAsync();
                
                Turn curTurn = new Turn(j, Players);
                string roundStateJson = JsonSerializer.Serialize(curTurn);
                Console.WriteLine(roundStateJson);
                
                await Task.Delay(6000);
            }
        }
        
        /// <summary>
        /// Démarre la partie de jeu de manière asynchrone.
        /// Appelée avec StartGameAsync(), elle vérifie d'abord que la carte et la liste des joueurs sont valides, puis lance la boucle de jeu.
        /// Retourne une Task.
        /// </summary>
        public async Task StartGameAsync()
        {
            await GameLoopAsync();
        }
        
        /// <summary>
        /// Traite une entrée unique d'un joueur et exécute l'action correspondante sur ce joueur.
        /// Appelée avec ReadInput(Player player, char i, Map carte), retourne void.
        /// Selon la valeur du caractère, elle effectue une rotation à gauche, un déplacement vers l'avant, un déplacement vers l'arrière ou une rotation à droite.
        /// </summary>
        /// <param name="player">Le joueur dont l'input est à traiter.</param>
        /// <param name="i">Le caractère d'entrée (par exemple 'a', 'z', 's', 'e').</param>
        /// <param name="carte">La carte sur laquelle se déroule l'action.</param>
        public void ReadInput(Player player, char i, Map carte)
        {
            if (i == 'a')
            {
                player.RotateLeft();
            }
            else if (i == 'z')
            {
                player.MoveForward(carte);
            }
            else if (i == 's')
            {
                player.MoveBackward(carte);
            }
            else if (i == 'e')
            {
                player.RotateRight();
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
            
        }
        public void GameLoop()
        {
            map.printMap(Players);
            foreach (var player in Players)
            {
                player.xA = player.x;
                player.yA = player.y;
                if (player == null)
                {
                    Console.WriteLine("Game: A player in the list is null.");
                    continue;
                }
                player.inputs = player.EnterInput(player);
            }
            for (int j = 0; j < 6; j++)
            {
                foreach (var player in Players)
                {
                    ReadInput(player, player.inputs[j], map);
                }
                foreach(var qPlayer in Players)
                {
                    foreach(var wPlayer in Players)
                    {
                        if (wPlayer.x == qPlayer.x && wPlayer.y == qPlayer.y && wPlayer.id != qPlayer.id)
                        {
                            qPlayer.HandleCollision(wPlayer,qPlayer, map, Players);
                        }
                    }
                }
                map.printMap(Players);
                Console.WriteLine("enter anything to continue");
                Console.ReadLine();
            }
        }
    }
}
