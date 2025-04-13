using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using tiz_teh_final_csharp_project;
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
                new Player { Id = 1 },
                new Player { Id = 2 }
            };

            string gameId = Guid.NewGuid().ToString();
            var game = new Game(mapPath, players, gameId);

            // Act
            // Submit moves for both players (6 turns in the round)
            // Player 1: Move forward, turn left, move forward, turn right, move forward, turn right
            game.SubmitPlayerInput(1, "zazes");
            // Player 2: Turn right, move forward, turn left, move forward, turn left, move forward
            game.SubmitPlayerInput(2, "ezaza");

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
                }
            }

            // Print the game state for visual verification
            PrintGameState(gameStatus);
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

        private void PrintGameState(GameStatusResponse gameStatus)
        {
            _testOutputHelper.WriteLine($"Game ID: {gameStatus.GameId}");
            _testOutputHelper.WriteLine($"Round: {gameStatus.CurrentRound} (Completed: {gameStatus.RoundState.RoundNumber + 1})");
            _testOutputHelper.WriteLine($"Game Over: {gameStatus.GameOver}");
            _testOutputHelper.WriteLine($"Map Size: {gameStatus.RoundState.Map.Width}x{gameStatus.RoundState.Map.Height}");

            _testOutputHelper.WriteLine("\nTurns:");
            foreach (var turn in gameStatus.RoundState.Turns)
            {
                _testOutputHelper.WriteLine($"  Turn {turn.TurnNumber}:");
                foreach (var player in turn.Players)
                {
                    _testOutputHelper.WriteLine($"    Player {player.Id}: Position ({player.X},{player.Y}) " +
                                                $"Direction: {player.Direction} Previous: ({player.PreviousX},{player.PreviousY})");
                }
            }
        }
    }
}