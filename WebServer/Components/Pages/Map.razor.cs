namespace BlazorApp1.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BlazorApp1.Class;
using System.Text.Json;
using System.IO;

public partial class Map : ComponentBase
{
    // variables utilisées entre le html et le code
    private BlazorApp1.Class.Map? _mapTotal;
    private IBrowserFile? _selectedFile;
    private string _mapJsonText = "";
    private string? _errorMessage;

    // variables pour modifier le comportement de l'affiachage
    private int _tileSize = 50;
    private bool _showCoordinates;
    private Tile? _selectedTile;

    // players logic
    private List<Player> _players = new();
    private Player? _selectedPlayer;
    private List<(int X, int Y)> _spawnPoints = new();

    // surper of jsonSerial to customise output format
    private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    // gets the file selection to the code of the page
    public void HandleFileSelection(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
        _errorMessage = null;
        Console.WriteLine($"File selected: {_selectedFile.Name}");
    }

    // fonctions visuelles de la carte
    private void ZoomIn()
    {
        _tileSize = Math.Min(100, _tileSize + 10);
    }

    private void ZoomOut()
    {
        _tileSize = Math.Max(20, _tileSize - 10);
    }

    private void ShowTileDetails(Tile tile)
    {
        _selectedTile = tile;
    }

    // for imported files
    public async Task ProcessSelectedFile()
    {
        if (_selectedFile == null) return;

        _errorMessage = null;
        StateHasChanged();

        try
        {
            Console.WriteLine($"Processing file: {_selectedFile.Name}");
            // opens the file 1Mb limit
            using var stream = _selectedFile.OpenReadStream(maxAllowedSize: 1024 * 1024);
            using var memoryStream = new MemoryStream();
            // copy the content into a memory stream, simply said it just copies the data into memory
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // reading the json
            using var reader = new StreamReader(memoryStream);
            var json = await reader.ReadToEndAsync();

            // sneak a peek...
            Console.WriteLine($"JSON content: {json.Substring(0, Math.Min(100, json.Length))}...");

            // deserialize to use the data
            _mapTotal = JsonSerializer.Deserialize<BlazorApp1.Class.Map>(json, _jsonOptions);

            if (_mapTotal == null)
            {
                _errorMessage = "Failed to parse JSON: Invalid format";
                Console.WriteLine("Map deserialized to null");
            }
            else
            {
                // sneek another peek... this time formated
                Console.WriteLine($"Map loaded: {_mapTotal.MapWidth}x{_mapTotal.MapHeight}, {_mapTotal.Tiles.Count} tiles");

                // after successful loading we check spawn points and initialise players
                FindSpawnPoints();
                InitializePlayers();
                Console.WriteLine($"Spawn points found: {_spawnPoints.Count}");
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
            Console.WriteLine($"Error loading map: {ex}");
            _mapTotal = null;
        }
        finally
        {
            // reload interface to show new map, and remove the file from the memory since it's been treated
            _selectedFile = null;
            StateHasChanged();
        }
    }

    // for pasted map content is limited to 10*10 for unknown reasons, surely because of text size but this is speculation
    public void ParseMapFromText()
    {
        if (string.IsNullOrWhiteSpace(_mapJsonText)) return;

        _errorMessage = null;
        StateHasChanged();

        try
        {
            // same as above but with less steps, yes they could be combined but hey...
            Console.WriteLine($"Parsing JSON text: {_mapJsonText.Substring(0, Math.Min(100, _mapJsonText.Length))}...");
            _mapTotal = JsonSerializer.Deserialize<BlazorApp1.Class.Map>(_mapJsonText, _jsonOptions);

            if (_mapTotal == null)
            {
                _errorMessage = "Failed to parse JSON: Invalid format";
                Console.WriteLine("Map deserialized to null");
            }
            else
            {
                Console.WriteLine($"Map loaded: {_mapTotal.MapWidth}x{_mapTotal.MapHeight}, {_mapTotal.Tiles.Count} tiles");

                // Après le succès du chargement
                FindSpawnPoints();
                InitializePlayers();
                Console.WriteLine($"Spawn points found: {_spawnPoints.Count}");
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
            Console.WriteLine($"Error parsing JSON: {ex}");
            _mapTotal = null;
        }
        finally
        {
            Console.Write("What is happening ?");
            StateHasChanged();
        }
    }

    // helper function to get the image of every tile 
    private string GetTileImagePath(string tileType)
    {
        return $"images/Tiles/{tileType.ToLowerInvariant()}.png";
    }

    // needs to check every tile
    private void FindSpawnPoints()
    {
        _spawnPoints.Clear();
        if (_mapTotal?.Tiles != null)
        {
            foreach (var tile in _mapTotal.Tiles)
            {
                if (tile.Type == "spawn")
                {
                    _spawnPoints.Add((tile.PosX, tile.PosY));
                }
            }
        }
    }

    // spawns players on found spawn points
    private void InitializePlayers()
    {
        _players.Clear();

        {
            foreach (var spawnPoint in _spawnPoints)
            {
                // limit to one for test reasons
                if (_players.Count >= 1)
                {
                    return;
                }
                // todo : change this placeholder number
                int placeholder = 1; 
                // here we count a new player in the game
                _players.Add(new Player(placeholder, spawnPoint.X, spawnPoint.Y, "Player1", _mapTotal));
            }
        }
    }

    // simple function to selection a player and update which player is affected by the movements, only one is spawned at the time of this comment
    private void SelectPlayer(Player player)
    {
        _selectedPlayer = player;
    }


    // Update movement methods to trigger shot movement
    private void MoveForward()
    {
        if (_selectedPlayer != null)
        {
            _selectedPlayer.MoveForward(true);
            _selectedPlayer.MoveShots();
            StateHasChanged();
        }
    }

    private void MoveBackward()
    {
        if (_selectedPlayer != null)
        {
            _selectedPlayer.MoveForward(false);
            _selectedPlayer.MoveShots();
            StateHasChanged();
        }
    }

    private void RotateRight()
    {
        if (_selectedPlayer != null)
        {
            _selectedPlayer.RotateRight(true);
            _selectedPlayer.MoveShots();
            StateHasChanged();
        }
    }

    private void RotateLeft()
    {
        if (_selectedPlayer != null)
        {
            _selectedPlayer.RotateRight(false);
            _selectedPlayer.MoveShots();
            StateHasChanged();
        }
    }

    private void PlayerShot()
    {
        if (_selectedPlayer != null)
        {
            var shot = new Shot(_selectedPlayer.X, _selectedPlayer.Y, _selectedPlayer, _mapTotal);
            _selectedPlayer.Shots.Add(shot);
            Console.WriteLine($"Shot added here onto player id :{_selectedPlayer.Id}");
            Console.WriteLine($"Player shots count{_selectedPlayer.Shots.Count}");
            _selectedPlayer.MoveShots();
            StateHasChanged();
        }
    }
}