using System.Text.Json.Serialization;

namespace tiz_teh_final_csharp_project
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