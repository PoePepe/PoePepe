using System;
using System.Reflection;

namespace Poe.UI.Services;

public class DivinationCardImagePosition
{
    public static readonly Uri DivinationCardUri = new($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/divinationCard.png");

}