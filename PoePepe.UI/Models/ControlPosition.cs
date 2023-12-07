namespace PoePepe.UI.Models;

public class ControlPosition
{
    public ControlPosition(int left, int top)
    {
        Left = left;
        Top = top;
    }

    /// <summary>
    /// Gets the X co-ordinate.
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Gets the Y co-ordinate.
    /// </summary>
    public int Top { get; }
}