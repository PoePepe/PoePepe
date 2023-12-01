namespace Poe.UIW.Models;

public class Sound
{
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
    private readonly bool _isCustom;

    public static Sound Custom(string path)
    {
        return new Sound(path, true);
    }
}