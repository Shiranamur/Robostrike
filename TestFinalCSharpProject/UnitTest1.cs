using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using api;
using Xunit.Abstractions;

namespace TestFinalCSharpProject
{
    public class Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task RunGameRoundAndVerifyStateObject()
        {
            // Arrange
            string mapPath = CreateTestMap();
            var players = new List<Player>
            {
                new Player { Id = 1, health = 2 },
                new Player { Id = 2, health = 2 }
            };

            string gameId = Guid.NewGuid().ToString();
            var game = new Game(mapPath, players, gameId);

            // Act
            // Set up moves to create potential collisions and shots
            // Player 1: Move forward, shoot, move forward, turn right, move forward, turn right
            game.SubmitPlayerInput(1, "zdzez");
            // Player 2: Turn right, move forward, shoot, move forward, turn left, move forward
            game.SubmitPlayerInput(2, "ezdza");

            // Start the game and wait for one round to complete
            var gameTask = game.StartGameAsync();
            await Task.Delay(2000); // Allow time for processing

            // Get the final game state
            var gameStatus = new GameStatusResponse
            {
                GameId = gameId,
                CurrentRound = game.CurrentRound,
                RoundState = game.GetCurrentRoundState(),
                GameOver = game.GameOver
            };

            // Assert
            Assert.Equal(gameId, gameStatus.GameId);
            Assert.Equal(1, gameStatus.CurrentRound); // Round 1 after completion
            Assert.False(gameStatus.GameOver);

            // Verify round state
            Assert.NotNull(gameStatus.RoundState.Map);
            Assert.Equal(6, gameStatus.RoundState.Turns.Count); // Should have 6 turns

            // Verify turn states
            foreach (var turn in gameStatus.RoundState.Turns)
            {
                Assert.Equal(2, turn.Players.Count); // Should have 2 players
                foreach (var playerState in turn.Players)
                {
                    Assert.Contains(playerState.Id, new[] { 1, 2 });
                    // Check positions are within map bounds
                    Assert.True(playerState.X >= 0);
                    Assert.True(playerState.X < gameStatus.RoundState.Map.Width);
                    Assert.True(playerState.Y >= 0);
                    Assert.True(playerState.Y < gameStatus.RoundState.Map.Height);
                    
                    // Check health value range
                    Assert.True(playerState.Health >= 0 && playerState.Health <= 2);
                }
            }

            // Print the game state for visual verification
            PrintGameState(gameStatus);
        }

        [Fact]
        public async Task TestCollisionsAndShots()
        {
            // Arrange - create a small map to increase chance of collisions
            string mapPath = CreateTestMap();
            var players = new List<Player>
            {
                new Player { Id = 1, health = 2 },
                new Player { Id = 2, health = 2 }
            };

            string gameId = Guid.NewGuid().ToString();
            var game = new Game(mapPath, players, gameId);

            // Log initial player positions before setting them
            _testOutputHelper.WriteLine("Default player positions after game creation:");
            foreach (var player in game.Players)
            {
                _testOutputHelper.WriteLine($"Player {player.Id}: ({player.X},{player.Y}) facing {player.Direction}");
            }

            // Position players exactly to create a guaranteed shot opportunity
            // Player 1 is right behind Player 2 and will shoot Player 2
            game.Players[0].X = 2;
            game.Players[0].Y = 4;
            game.Players[0].Direction = 'N';

            game.Players[1].X = 2;
            game.Players[1].Y = 3;
            game.Players[1].Direction = 'S';

            // Log player positions after setting them
            _testOutputHelper.WriteLine("\nCustom player positions before game start:");
            foreach (var player in game.Players)
            {
                _testOutputHelper.WriteLine($"Player {player.Id}: ({player.X},{player.Y}) facing {player.Direction}");
            }
            
            // Act - run the game
            var gameTask = game.StartGameAsync();

            // Submit moves for the forced scenario
            // Player 1: Shoot (should hit Player 2), then other moves
            game.SubmitPlayerInput(1, "zszzz");
            // Player 2: Shoot (should hit Player 1), then other moves
            game.SubmitPlayerInput(2, "sszzzz");

            await Task.Delay(4000);
            
            // Get the game state after round completion
            var gameStatus = new GameStatusResponse
            {
                GameId = gameId,
                CurrentRound = game.CurrentRound,
                RoundState = game.GetCurrentRoundState(),
                GameOver = game.GameOver
            };

            // Print map visualization for each turn
            PrintMapVisualization(gameStatus);
            
            // Print detailed state
            PrintDetailedGameState(gameStatus);

            // Custom assertions to specifically check shot mechanics
            var turn0 = gameStatus.RoundState.Turns[0];
            _testOutputHelper.WriteLine("\nAssertions for Turn 0:");
            _testOutputHelper.WriteLine($"Player 1 shot hit: {turn0.Players[0].ShotHitPlayer.Count > 0}");
            _testOutputHelper.WriteLine($"Player 2 shot hit: {turn0.Players[1].ShotHitPlayer.Count > 0}");
            _testOutputHelper.WriteLine($"Player 1 health: {turn0.Players[0].Health}, Damage taken: {turn0.Players[0].Damage_Taken}");
            _testOutputHelper.WriteLine($"Player 2 health: {turn0.Players[1].Health}, Damage taken: {turn0.Players[1].Damage_Taken}");
        }

        private void PrintMapVisualization(GameStatusResponse gameStatus)
        {
            _testOutputHelper.WriteLine("\nMap Visualizations for each turn:");
            
            int mapWidth = gameStatus.RoundState.Map.Width;
            int mapHeight = gameStatus.RoundState.Map.Height;
            
            foreach (var turn in gameStatus.RoundState.Turns)
            {
                _testOutputHelper.WriteLine($"\nTurn {turn.TurnNumber}:");
                
                // Create a map representation
                char[,] mapDisplay = new char[mapHeight, mapWidth];
                
                // Fill with empty spaces
                for (int y = 0; y < mapHeight; y++)
                    for (int x = 0; x < mapWidth; x++)
                        mapDisplay[y, x] = '.';
                        
                // Add obstacles
                foreach (var tile in gameStatus.RoundState.Map.tiles)
                {
                    if (tile.Type == "obstacle")
                        mapDisplay[tile.Y, tile.X] = '#';
                }
                
                // Add players
                foreach (var player in turn.Players)
                {
                    if (player.X >= 0 && player.X < mapWidth && player.Y >= 0 && player.Y < mapHeight)
                        mapDisplay[player.Y, player.X] = player.Id.ToString()[0];
                }
                
                // Print the map
                for (int y = 0; y < mapHeight; y++)
                {
                    var line = new System.Text.StringBuilder();
                    for (int x = 0; x < mapWidth; x++)
                    {
                        line.Append(mapDisplay[y, x]);
                    }
                    _testOutputHelper.WriteLine(line.ToString());
                }
                
                // Print player legend
                foreach (var player in turn.Players)
                {
                    _testOutputHelper.WriteLine($"Player {player.Id}: ({player.X},{player.Y}) facing {player.Direction}");
                }
            }
        }
        
        private string CreateTestMap()
        {
            // Create a simple test map
            var map = new Map(10, 10, new List<Tile>());

            // Add tiles (all walkable except some obstacles)
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    string tileType = "ground";
                    if ((x == 3 && y == 3) || (x == 7 && y == 7))
                    {
                        tileType = "obstacle";
                    }

                    map.tiles.Add(new Tile { X = x, Y = y, Type = tileType });
                }
            }

            // Save map to temporary file
            string mapPath = Path.GetTempFileName();
            File.WriteAllText(mapPath, JsonSerializer.Serialize(map));
            return mapPath;
        }

        private string CreateSmallTestMap()
        {
            // Create a small map to increase chance of player interaction
            var map = new Map(5, 5, new List<Tile>());

            // Add tiles (all walkable except edge obstacles)
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    string tileType = "ground";
                    // Add obstacles around edges
                    if (x == 0 || y == 0 || x == 4 || y == 4)
                    {
                        tileType = "obstacle";
                    }

                    map.tiles.Add(new Tile { X = x, Y = y, Type = tileType });
                }
            }

            string mapPath = Path.GetTempFileName();
            File.WriteAllText(mapPath, JsonSerializer.Serialize(map));
            return mapPath;
        }

        private void PrintGameState(GameStatusResponse gameStatus)
        {
            _testOutputHelper.WriteLine($"Game ID: {gameStatus.GameId}");
            _testOutputHelper.WriteLine($"Round: {gameStatus.CurrentRound} (Completed: {gameStatus.RoundState.RoundNumber})");
            _testOutputHelper.WriteLine($"Game Over: {gameStatus.GameOver}");
            _testOutputHelper.WriteLine($"Map Size: {gameStatus.RoundState.Map.Width}x{gameStatus.RoundState.Map.Height}");

            _testOutputHelper.WriteLine("\nTurns:");
            foreach (var turn in gameStatus.RoundState.Turns)
            {
                _testOutputHelper.WriteLine($"  Turn {turn.TurnNumber}:");
                foreach (var player in turn.Players)
                {
                    _testOutputHelper.WriteLine($"    Player {player.Id}: Position ({player.X},{player.Y}) " +
                                                $"Direction: {player.Direction} Health: {player.Health}");
                }
            }
        }
private void PrintDetailedGameState(GameStatusResponse gameStatus)
        {
            _testOutputHelper.WriteLine($"Game ID: {gameStatus.GameId}");
            _testOutputHelper.WriteLine($"Round: {gameStatus.CurrentRound}");
        
            _testOutputHelper.WriteLine("\nDETAILED TURN-BY-TURN ANALYSIS:");
            foreach (var turn in gameStatus.RoundState.Turns)
            {
                _testOutputHelper.WriteLine($"\n==== TURN {turn.TurnNumber} ====");
                
                // Print a summary first
                _testOutputHelper.WriteLine("SUMMARY:");
                foreach (var player in turn.Players)
                {
                    string statusIndicator = player.IsAlive ? "🟢" : "🔴";
                    string damageIndicator = player.Damage_Taken > 0 ? $"(-{player.Damage_Taken})" : "";
                    string shotIndicator = player.ShotHitPlayer != null && player.ShotHitPlayer.Count > 0 ? "🎯" : "";
                    string collisionIndicator = !string.IsNullOrEmpty(player.CollisionType) ? "💥" : "";
                    
                    _testOutputHelper.WriteLine($"  Player {player.Id}: {statusIndicator} Health: {player.Health}{damageIndicator} | Pos: ({player.X},{player.Y}) Dir: {player.Direction} {shotIndicator}{collisionIndicator}");
                }
                
                // Now detailed player status
                _testOutputHelper.WriteLine("\nDETAILED PLAYER STATUS:");
                foreach (var player in turn.Players)
                {
                    _testOutputHelper.WriteLine($"  PLAYER {player.Id} STATUS:");
                    _testOutputHelper.WriteLine($"    Position: ({player.X},{player.Y})");
                    _testOutputHelper.WriteLine($"    Direction: {player.Direction}");
                    _testOutputHelper.WriteLine($"    Health: {player.Health}/{(player.IsAlive ? "Alive" : "Dead")}");
                    _testOutputHelper.WriteLine($"    Damage This Turn: {player.Damage_Taken}");
                    
                    // Shot tracking
                    _testOutputHelper.WriteLine("    SHOTS:");
                    if (player.ShotHitPlayer != null && player.ShotHitPlayer.Count > 0)
                    {
                        foreach (var target in player.ShotHitPlayer)
                        {
                            _testOutputHelper.WriteLine($"      ✓ Hit Player {target.Key} at:");
                            foreach (var coord in target.Value)
                            {
                                _testOutputHelper.WriteLine($"        • ({coord.Key}, {coord.Value})");
                            }
                        }
                    }
                    else
                    {
                        _testOutputHelper.WriteLine("      × No shots hit");
                    }
                    
                    // Collision tracking
                    _testOutputHelper.WriteLine("    COLLISIONS:");
                    if (!string.IsNullOrEmpty(player.CollisionType))
                    {
                        _testOutputHelper.WriteLine($"      ✓ Type: {player.CollisionType}");
                        if (player.CollisionWithId > 0)
                        {
                            _testOutputHelper.WriteLine($"      ✓ Collided with: Player {player.CollisionWithId}");
                        }
                        if (player.CollisionCoordinates != null && player.CollisionCoordinates.Count > 0)
                        {
                            foreach (var coord in player.CollisionCoordinates)
                            {
                                _testOutputHelper.WriteLine($"      ✓ At coordinates: ({coord.Key}, {coord.Value})");
                            }
                        }
                    }
                    else
                    {
                        _testOutputHelper.WriteLine("      × No collisions");
                    }
                    
                    _testOutputHelper.WriteLine("");
                }
                
                // Track action outcomes
                _testOutputHelper.WriteLine("ACTION OUTCOMES:");
                bool anyActions = false;
                foreach (var player in turn.Players)
                {
                    if ((player.ShotHitPlayer != null && player.ShotHitPlayer.Count > 0) || 
                        !string.IsNullOrEmpty(player.CollisionType))
                    {
                        anyActions = true;
                        _testOutputHelper.WriteLine($"  Player {player.Id}:");
                        
                        if (player.ShotHitPlayer != null && player.ShotHitPlayer.Count > 0)
                        {
                            _testOutputHelper.WriteLine("    SHOT OUTCOMES:");
                            foreach (var target in player.ShotHitPlayer)
                            {
                                _testOutputHelper.WriteLine($"    • Shot hit Player {target.Key}");
                                // You could calculate damage here if needed
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(player.CollisionType))
                        {
                            _testOutputHelper.WriteLine("    COLLISION OUTCOMES:");
                            _testOutputHelper.WriteLine($"    • {player.CollisionType} collision" + 
                                (player.CollisionWithId > 0 ? $" with Player {player.CollisionWithId}" : ""));
                            // You could calculate collision effects here if needed
                        }
                    }
                }
                
                if (!anyActions)
                {
                    _testOutputHelper.WriteLine("  No significant actions this turn");
                }
            }
        }
    }
}