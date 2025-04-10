namespace tiz_teh_final_csharp_project;

public class MapHelper
{
    private readonly string _mapDirectory;

    public MapHelper()
    {
        // Get the current executing assembly path (bin/Debug/net9.0)
        string binDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Try to get directory from configuration, or use fallback paths
        _mapDirectory = Path.GetFullPath(Path.Combine(binDirectory, "../../../Map"));

        Console.WriteLine($"[Debug] Map directory path: {_mapDirectory}");
    
        // Ensure directory exists
        if (!Directory.Exists(_mapDirectory))
        {
            Directory.CreateDirectory(_mapDirectory);
            Console.WriteLine($"[Debug] Created map directory: {_mapDirectory}");
        }
    }

    public string PickRandomMap()
    {
        if (!Directory.Exists(_mapDirectory))
        {
            throw new DirectoryNotFoundException($"Directory {_mapDirectory} not found");
        }
        
        var mapFiles = Directory.GetFiles(_mapDirectory, "*.json");
        if (mapFiles == null || mapFiles.Length == 0)
        {
            throw new Exception("pas de map dans l'dossier chef");
        }
        var random = new Random();
        int Index = random.Next(mapFiles.Length);
        return mapFiles[Index];
    }
}