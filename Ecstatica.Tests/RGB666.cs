namespace Ecstatica.Tests;

public readonly struct RGB666 : IEquatable<RGB666>
{
    public readonly byte R, G, B;

    public RGB666(int r, int g, int b)
    {
        if (r is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(r));
        }

        if (g is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(g));
        }

        if (b is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(b));
        }

        R = (byte)r;
        G = (byte)g;
        B = (byte)b;
    }


    public override string ToString()
    {
        return $"{nameof(R)}: {R}, {nameof(G)}: {G}, {nameof(B)}: {B}";
    }

    public bool Equals(RGB666 other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is RGB666 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public static bool operator ==(RGB666 left, RGB666 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RGB666 left, RGB666 right)
    {
        return !left.Equals(right);
    }

    public RGB555 ToRGB555()
    {
        var rgb555 = new RGB555(
            BitExtensions.Convert6BitTo5Bit(R),
            BitExtensions.Convert6BitTo5Bit(G),
            BitExtensions.Convert6BitTo5Bit(B)
        );

        return rgb555;
    }

    public RGB888 ToRGB888()
    {
        var rgb888 = new RGB888(
            BitExtensions.Convert6BitTo8Bit(R),
            BitExtensions.Convert6BitTo8Bit(G),
            BitExtensions.Convert6BitTo8Bit(B)
        );

        return rgb888;
    }
}