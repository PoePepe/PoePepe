namespace Poe.UIW.Models;

public class ControlPosition
{
    /// <summary>
    /// Gets the X co-ordinate.
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Gets the Y co-ordinate.
    /// </summary>
    public int Top { get; }

    public ControlPosition(int left, int top)
    {
        Left = left;
        Top = top;
    }
}