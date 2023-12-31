namespace Ecstatica.Tests;

public readonly struct RGB888 : IEquatable<RGB888>
{
    public readonly byte R, G, B;

    public RGB888(int r, int g, int b)
    {
        if (r is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(r));
        }

        if (g is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(g));
        }

        if (b is < 0 or > 255)
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

    public bool Equals(RGB888 other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is RGB888 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public static bool operator ==(RGB888 left, RGB888 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RGB888 left, RGB888 right)
    {
        return !left.Equals(right);
    }

    public RGB555 ToRGB555()
    {
        var rgb555 = new RGB555(
            BitExtensions.Convert8BitTo5Bit(R),
            BitExtensions.Convert8BitTo5Bit(G),
            BitExtensions.Convert8BitTo5Bit(B)
        );

        return rgb555;
    }

    public RGB666 ToRGB666()
    {
        var rgb666 = new RGB666(
            BitExtensions.Convert8BitTo6Bit(R),
            BitExtensions.Convert8BitTo6Bit(G),
            BitExtensions.Convert8BitTo6Bit(B)
        );

        return rgb666;
    }
}