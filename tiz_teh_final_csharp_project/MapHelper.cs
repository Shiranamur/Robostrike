namespace tiz_teh_final_csharp_project;

public class MapHelper
{
    private readonly string _mapDirectory;

    public MapHelper()
    {
        _mapDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Map");
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