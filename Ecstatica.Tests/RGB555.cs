namespace Ecstatica.Tests;

public readonly struct RGB555 : IEquatable<RGB555>
{
    public readonly byte R, G, B;

    public RGB555(int r, int g, int b)
    {
        if (r is < 0 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(r));
        }

        if (g is < 0 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(g));
        }

        if (b is < 0 or > 31)
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

    public bool Equals(RGB555 other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is RGB555 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public static bool operator ==(RGB555 left, RGB555 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RGB555 left, RGB555 right)
    {
        return !left.Equals(right);
    }

    public RGB666 ToRGB666()
    {
        var rgb666 = new RGB666(
            BitExtensions.Convert5BitTo6Bit(R),
            BitExtensions.Convert5BitTo6Bit(G),
            BitExtensions.Convert5BitTo6Bit(B)
        );

        return rgb666;
    }

    public RGB888 ToRGB888()
    {
        var rgb888 = new RGB888(
            BitExtensions.Convert5BitTo8Bit(R),
            BitExtensions.Convert5BitTo8Bit(G),
            BitExtensions.Convert5BitTo8Bit(B)
        );

        return rgb888;
    }
}