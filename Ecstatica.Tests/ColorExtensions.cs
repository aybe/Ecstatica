using System.Windows.Media;
using JetBrains.Annotations;

namespace Ecstatica.Tests;

[PublicAPI]
public static class ColorExtensions
{
    public static Color ToColor(this RGB555 value)
    {
        var color = value.ToRGB888().ToColor();

        return color;
    }

    public static Color ToColor(this RGB666 value)
    {
        var color = value.ToRGB888().ToColor();

        return color;
    }

    public static Color ToColor(this RGB888 value)
    {
        var color = Color.FromRgb(value.R, value.G, value.B);

        return color;
    }
}