namespace PoePepe.UI.Models;

public class Sound
{
    private readonly bool _isCustom;

    public Sound(string path)
    {
        Path = path;
        _isCustom = false;
    }

    private Sound(string path, bool isCustom = true)
    {
        Path = path;
        _isCustom = isCustom;
    }

    public string Name
    {
        get
        {
            var name = Path?[..Path.LastIndexOf('.')];
            return _isCustom ? $"Custom {name}" : name;
        }
    }

    public string Path { get; set; }

    public static Sound Custom(string path) => new(path, true);
}