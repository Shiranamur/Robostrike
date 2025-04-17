namespace BlazorApp1.Class;
using System.Text.Json.Serialization;

public class Tile
{
    [JsonPropertyName("x")] public int PosX { get; set; }
    [JsonPropertyName("y")] public int PosY { get; set; }
    [JsonPropertyName("type")] public required string Type { get; set; }
}