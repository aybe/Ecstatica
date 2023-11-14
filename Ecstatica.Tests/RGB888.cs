namespace Ecstatica.Tests;

public readonly struct RGB888 : IEquatable<RGB888>
{
    public readonly byte R, G, B;

    public RGB888(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
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
}