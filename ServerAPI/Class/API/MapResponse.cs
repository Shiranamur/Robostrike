namespace ServerAPI.Class.API;

public class MapResponse
{
    public string Message { get; set; } = string.Empty;
    public Map? MapDetails { get; set; }
}