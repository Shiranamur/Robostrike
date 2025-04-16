using System.Text.Json.Serialization;

namespace api
{
    public class Tile
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }
    }
    
}