namespace ServerAPI.Class;
using System.Text.Json.Serialization;

public class Map
{
    [JsonPropertyName("map_width")] public int MapWidth { get; init; }
    [JsonPropertyName("map_height")] public int MapHeight { get; init; }
    [JsonPropertyName("tiles")] public List<Tile> Tiles { get; init; } = new List<Tile>();
}
