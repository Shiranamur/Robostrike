using System.Text.Json;
using System.Collections.Concurrent;

namespace tiz_teh_final_csharp_project
{
    public class GameStatusResponse
    {
        public string GameId { get; set; }
        public int CurrentRound { get; set; }
        public RoundState RoundState { get; set; }
        public bool GameOver { get; set; }
    }

    public class RoundState
    {
        public int RoundNumber { get; set; }
        public List<TurnState> Turns { get; set; } = new List<TurnState>();
        public Map Map { get; set; }
    }

    public class TurnState
    {
        public int TurnNumber { get; set; }
        public List<PlayerState> Players { get; set; } = new List<PlayerState>();
    }

    public class PlayerState
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public char Direction { get; set; }
        public bool IsAlive { get; set; }
        
        public Dictionary<int, Dictionary<int, int>> ShotHitPlayer = new Dictionary<int, Dictionary<int, int>>();
        public string CollisionType { get; set; }
        public Dictionary<int, int> CollisionCoordinates = new Dictionary<int, int>();
        public int CollisionWithId  { get; set;}
        public char Input { get; set; }        

        public int Health { get; set; }
        public int Damage_Taken { get; set; }
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

        public int CurrentRound = 0;
        
        public bool GameOver = false;

        private const int _maxRounds = 6;

        private const int _maxTurns = 6;

        // Stocke les inputs des joueurs pour chaque round.
        // Key: numéro de round ; Value: dictionnaire associant l'id du joueur à son caractère d'entrée.
        private ConcurrentDictionary<int, Dictionary<int, char>> _roundInputs = new();
        
        // Track which players have submitted inputs for the current round
        private readonly ConcurrentDictionary<int, HashSet<int>> _submittedInputs = new();


        // Stocke un TaskCompletionSource pour chaque round afin d'attendre que tous les joueurs aient soumis leur input.
        private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _roundInputCompletionSources = new();

        // buffer list to contain turns for the round class
        private List<TurnState> _currentRoundTurns = new List<TurnState>();
        
        private readonly Dictionary<int, RoundState> _completedRoundStates = new Dictionary<int, RoundState>();

        private Dictionary<int, (string type, int withId, Dictionary<int, int> coordinates)> _currentTurnCollisions = 
            new Dictionary<int, (string, int, Dictionary<int, int>)>();
        
        private Dictionary<int, Dictionary<int, Dictionary<int, int>>> _currentTurnShots = 
            new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
        
        /// <summary>
        /// Constructeur de la classe Game.
        /// Charge la carte à partir d'un fichier JSON et initialise la liste des joueurs.
        /// Appelé avec Game(string mapFile, List(Player) players) et retourne une instance de Game initialisée.
        /// </summary>
        /// <param name="mapFile">Le chemin du fichier JSON contenant la carte.</param>
        /// <param name="players">La liste minimale des joueurs (avec uniquement leur identifiant, qui sera enrichi).</param>
        /// <param name="matchId">Id du match crée par gameManager</param>
        public Game(string mapFile, List<Player> players, string matchId)
        {
            this.MatchId = matchId;
            
            if (!File.Exists(mapFile))
            {
                Console.WriteLine($"Game: Map file not found at {mapFile}");
                return;
            }
            
            string json = File.ReadAllText(mapFile);
            map = JsonSerializer.Deserialize<Map>(json);

            if (map == null)
            {
                Console.WriteLine($"Game: Map file is empty at {mapFile}");
                throw new ArgumentNullException(nameof(mapFile), "Could not deserialize map file");
            }

            Console.WriteLine($"Map initialized with Width: {map.Width}, Height: {map.Height}, Tiles: {map.tiles.Count}");
            
            if (players == null || players.Count == 0)
            {
                Console.WriteLine("Game: Players list is null or empty.");
                throw new ArgumentNullException(nameof(players), "Players list is null or empty.");
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
        // TODO : add spawn support in here
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

                Players[i].X = spacingX * (col + 1);
                Players[i].Y = spacingY * (row + 1);
                Players[i].Direction = 'S';  // Direction par défaut (Sud)
            }
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].InGameId = i;
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
                    id = p.Id,
                    inGameId = p.InGameId, 
                    x = p.X,
                    y = p.Y,
                    direction = p.Direction,
                    curInput = p.CurInput,
                }).ToList()
            };
        }

        
        /// <summary>
        /// Soumet une list d'entrées de joueur pour un round donné.
        /// Appelée avec SubmitPlayerInput(int playerId, string input) et retourne void.
        /// Elle enregistre l'input du joueur et, si tous les inputs sont reçus, déclenche la complétion de la tâche d'attente.
        /// est trigger par : PUT /api/game/{matchId}/inputs
        /// </summary>
        /// <param name="playerId">L'identifiant du joueur.</param>
        /// <param name="inputs">L'entrée saisie par le joueur.</param>
        public void SubmitPlayerInput(int playerId, string inputs)
        {
            // Store each turn's input for this player
            for (int turn = 0; turn < _maxTurns; turn++)
            {
                // Get or create the input dictionary for this turn
                var turnInputs = _roundInputs.GetOrAdd(turn, _ => new Dictionary<int, char>());
        
                // Lock only the specific turn's dictionary for thread safety
                lock (turnInputs)
                {
                    turnInputs[playerId] = turn < inputs.Length ? inputs[turn] : ' ';
                }
            }

            // Add player to the set of players who have submitted inputs for this round
            _submittedInputs.GetOrAdd(CurrentRound, _ => new HashSet<int>()).Add(playerId);

            // Check if all players have submitted inputs for current round
            CheckRoundInputsCompletion();
        }

        
        /// <summary>
        /// Checks if all players have submitted inputs for the current round.
        /// If all players have submitted, signals the corresponding TaskCompletionSource to continue processing.
        /// </summary>
        private void CheckRoundInputsCompletion()
        {
            // Get the set of players who submitted for this round
            if (_submittedInputs.TryGetValue(CurrentRound, out var submittedPlayers))
            {
                lock (submittedPlayers)
                {
                    // Check if all players have submitted
                    if (submittedPlayers.Count >= Players.Count &&
                        _roundInputCompletionSources.TryGetValue(CurrentRound, out var tcs))
                    {
                        // Set the result and complete the waiting task
                        tcs.TrySetResult(true);
                    }
                }
            }
        }


        /// <summary>
        /// Attend de façon asynchrone que tous les joueurs aient soumis leurs inputs pour un round.
        /// Appelée avec WaitForRoundInputsAsync() et retourne une Task qui se complète quand tous les inputs sont reçus.
        /// Implémente également une fonction de timeout pour ne pas bloquer le jeu.
        /// </summary>
        private async Task WaitForRoundInputsAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            _roundInputCompletionSources[CurrentRound] = tcs;

            // Track which players have and haven't submitted
            var pendingPlayers = new HashSet<int>(Players.Select(p => p.Id));
    
            // Check if any players have already submitted
            if (_submittedInputs.TryGetValue(CurrentRound, out var submittedPlayers))
            {
                lock (submittedPlayers)
                {
                    foreach (var id in submittedPlayers)
                        pendingPlayers.Remove(id);
                }
            }

            // If all have submitted, complete immediately
            if (pendingPlayers.Count == 0)
            {
                tcs.TrySetResult(true);
                return;
            }

            // Add timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => {
                Console.WriteLine($"Round {CurrentRound} timed out waiting for players: {string.Join(", ", pendingPlayers)}");
        
                // For players who didn't submit, use a default input
                foreach (var playerId in pendingPlayers)
                {
                    FillDefaultInputs(playerId);
                }
        
                tcs.TrySetResult(true);
            });

            await tcs.Task.ConfigureAwait(false);
        }
        
        
        /// <summary>
        /// Fills in default inputs (space character) for a player who hasn't submitted inputs before the timeout.
        /// Used when a player fails to submit inputs within the allotted time.
        /// </summary>
        /// <param name="playerId">The ID of the player who needs default inputs</param>
        private void FillDefaultInputs(int playerId)
        {
            // Fill with space character (no action) for all turns
            for (int turn = 0; turn < _maxTurns; turn++)
            {
                var turnInputs = _roundInputs.GetOrAdd(turn, _ => new Dictionary<int, char>());
                lock (turnInputs)
                {
                    // Default input is space
                    turnInputs.TryAdd(playerId, ' ');
                }
            }
        }
        
        // Add this method to get round state
        // Update GetCurrentRoundState to return the most recent completed round
        public RoundState GetCurrentRoundState()
        {
            // If there's a completed round, return it
            if (_completedRoundStates.TryGetValue(CurrentRound > 0 ? CurrentRound - 1 : 0, out var completedState))
            {
                return completedState;
            }
        
            // Otherwise return current in-progress state
            return new RoundState
            {
                RoundNumber = CurrentRound,
                Turns = _currentRoundTurns,
                Map = this.map
            };
        }
        
        
        // Add method to get specific round state
        public RoundState GetRoundState(int roundNumber)
        {
            if (_completedRoundStates.TryGetValue(roundNumber, out var state))
            {
                return state;
            }
        
            return null;
        }
        
                
        public void RecordTurnState(int currentTurn)
        {
            var turnState = new TurnState
            {
                TurnNumber = currentTurn,
                Players = Players.Select(p => 
                {
                    var playerState = new PlayerState
                    {
                        Id = p.Id,
                        X = p.X,
                        Y = p.Y,
                        Direction = p.Direction,
                        IsAlive = p.IsAlive,
                        Health = p.health,
                        Damage_Taken = p.hit,
                        Input = _roundInputs.TryGetValue(currentTurn, out var turnDict) ? 
                            turnDict.GetValueOrDefault(p.Id, ' ') : ' ',
                        ShotHitPlayer = p.ShotHitPlayer
                    };
                    
                    // Add collision data if exists for this player
                    if (_currentTurnCollisions.ContainsKey(p.Id) && p.CollisionType != "")
                    {
                        playerState.CollisionType = p.CollisionType;
                        playerState.CollisionCoordinates = p.CollisionCoordinates;
                        playerState.CollisionWithId = p.CollisionWithId;
                    }
                    
                    return playerState;
                }).ToList()
            };
            _currentRoundTurns.Add(turnState);
        }
        
        /// <summary>
        /// Processes a single turn by applying player inputs and resolving collisions.
        /// Updates player positions based on their inputs and handles any resulting collisions between players.
        /// </summary>
        /// <param name="turnInputs">Dictionary mapping player IDs to their input characters for this turn</param>
        private void ProcessTurn(Dictionary<int, char> turnInputs)
        {
            // Clear previous turn's shot data
            _currentTurnShots.Clear();
            _currentTurnCollisions.Clear();

            foreach (var player in Players)
            {
                player.XOld = player.X;
                player.YOld = player.Y;
                player.ShotHitPlayer = new Dictionary<int, Dictionary<int, int>>();
                player.CollisionCoordinates = new Dictionary<int, int>();
                player.CollisionType = "";
                player.CollisionWithId = 0;

                if (player.IsAlive)
                {
                    ReadInput(player, turnInputs.GetValueOrDefault(player.Id, ' '), map);
                    Console.WriteLine(
                        $"Player : {player.Id} has input : {turnInputs.GetValueOrDefault(player.Id, ' ')}");
                }

            }
            foreach(var currentPlayer in Players)
            {
                foreach(var otherPlayer in Players)
                {
                    if (currentPlayer.Id == otherPlayer.Id) continue;
            
                    // Same position collision
                    if (otherPlayer.X == currentPlayer.X && otherPlayer.Y == currentPlayer.Y)
                    {
                        var collisionCoords = new Dictionary<int, int> {{currentPlayer.X, currentPlayer.Y}};
                        int result = currentPlayer.HandleCollision(currentPlayer, otherPlayer, map, Players);
                
                        RecordCollisionResult(result, currentPlayer, otherPlayer, collisionCoords);
                    }
                    // Check for players trying to swap positions
                    else if (IsPassThroughCollision(currentPlayer, otherPlayer))
                    {
                        var collisionCoords = new Dictionary<int, int> {{currentPlayer.XOld, currentPlayer.YOld}};
                
                        // Reset position to original location
                        currentPlayer.X = currentPlayer.XOld;
                        currentPlayer.Y = currentPlayer.YOld;
                
                        // Record collision info
                        _currentTurnCollisions[currentPlayer.Id] = ("stalemate", otherPlayer.Id, collisionCoords);
                    }
                }
            }
        }

        // Helper method to check for pass-through collisions
        private bool IsPassThroughCollision(Player player1, Player player2)
        {
            return player1.X == player2.XOld && player1.Y == player2.YOld &&
                   player2.X == player1.XOld && player2.Y == player1.YOld;
        }

        // Helper method to record collision results
        private void RecordCollisionResult(int result, Player currentPlayer, Player otherPlayer, Dictionary<int, int> coordinates)
        {
            if (result == 1) // Successful push
            {
                _currentTurnCollisions[currentPlayer.Id] = ("push", otherPlayer.Id, coordinates);
                _currentTurnCollisions[otherPlayer.Id] = ("pushed", currentPlayer.Id, coordinates);
            }
            else if (result == -1) // Other player has push priority
            {
                _currentTurnCollisions[otherPlayer.Id] = ("push", currentPlayer.Id, coordinates);
                _currentTurnCollisions[currentPlayer.Id] = ("pushed", otherPlayer.Id, coordinates);
            }
            else // result == 0, stalemate or failed push
            {
                _currentTurnCollisions[currentPlayer.Id] = ("stalemate", otherPlayer.Id, coordinates);
                _currentTurnCollisions[otherPlayer.Id] = ("stalemate", currentPlayer.Id, coordinates);
            }
        }
        
        public bool CheckGameEnd()
        {
            if (Players.Count <= 1)
            {
                return true;
            }

            return false;
        }
        
        
        /// <summary>
        /// Starts and runs the game asynchronously until completion.
        /// Processes rounds sequentially, waiting for player inputs (with timeout) before processing each round.
        /// </summary>
        /// <returns>A task that completes with true when the game is finished</returns>
        public async Task<bool> StartGameAsync()
        {
            Console.WriteLine("Started the game !");
            while (!GameOver && CurrentRound < _maxRounds) // continue game while not finished
            {
                Console.WriteLine($"Round : {CurrentRound}");
                _currentRoundTurns = new List<TurnState>(); // Create new instead of clearing
                
                _roundInputs = new ConcurrentDictionary<int, Dictionary<int, char>>();
                // wait inputs with timout
                await WaitForRoundInputsAsync();
               
                // process turns inside the round
                for (int turn = 0; turn < _maxTurns; turn++)
                {
                    Console.WriteLine($"Turn : {turn}");
                    // get the inputs for the turn
                    foreach (var player in Players)
                    {
                        player.hit = 0;
                    }
                    
                    Dictionary<int, char> turnInputs = _roundInputs.GetOrAdd(turn, new Dictionary<int, char>());
                    ProcessTurn(turnInputs); // process
                    RecordTurnState(turn);
                }
                

                // Save the completed round state
                _completedRoundStates[CurrentRound] = new RoundState
                {
                    RoundNumber = CurrentRound,
                    Turns = new List<TurnState>(_currentRoundTurns), // Create a copy
                    Map = this.map
                };
                
                CurrentRound++;                
            }
            Console.WriteLine("Game Is finished");
            return true; // game is finished
        }
        
        /// <summary>
        /// Traite une entrée unique d'un joueur et exécute l'action correspondante sur ce joueur.
        /// Appelée avec ReadInput(Player player, char i, Map carte), retourne void.
        /// Selon la valeur du caractère, elle effectue une rotation à gauche, un déplacement vers l'avant, un déplacement vers l'arrière ou une rotation à droite.
        /// </summary>
        /// <param name="player">Le joueur dont l'input est à traiter.</param>
        /// <param name="input">Le caractère d'entrée (par exemple 'a', 'z', 's', 'e').</param>
        /// <param name="carte">La carte sur laquelle se déroule l'action.</param>
        private void ReadInput(Player player, char input, Map carte)
        {
            if (input == 'a')
            {
                player.RotateLeft();
            }
            else if (input == 'z')
            {
                player.MoveForward(carte);
            }
            else if (input == 's')
            {
                player.MoveBackward(carte);
            }
            else if (input == 'e')
            {
                player.RotateRight();

            }
            else if (input == 'd')
            {
                var shotResults = player.Shoot(carte, Players);
                if (shotResults.Count > 0)
                {
                    _currentTurnShots[player.Id] = shotResults;
                    
                }
            }
            else if (input == ' ')
            {
                Console.WriteLine("Someone didn't submit their input...");
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}
