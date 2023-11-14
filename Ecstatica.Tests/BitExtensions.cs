namespace Ecstatica.Tests;

public static class BitExtensions
{
    public static byte Convert5BitTo6Bit(byte value)
    {
        var b = (byte)((value * 63 + 15) / 31);

        return b;
    }

    public static byte Convert5BitTo8Bit(byte value)
    {
        var b = (byte)((value * 255 + 15) / 31);

        return b;
    }

    public static byte Convert6BitTo5Bit(byte value)
    {
        var b = (byte)((value * 31 + 31) / 63);

        return b;
    }

    public static byte Convert6BitTo8Bit(byte value)
    {
        var b = (byte)((value * 255 + 31) / 63);

        return b;
    }

    public static byte Convert8BitTo5Bit(byte value)
    {
        var b = (byte)((value * (31 << 8) + 127) / 255);

        return b;
    }

    public static byte Convert8BitTo6Bit(byte value)
    {
        var b = (byte)((value * (63 << 8) + 127) / 255);

        return b;
    }

    public static byte GetByte(int value, int index)
    {
        if (index is < 0 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var b = (byte)((value >> (index * 8)) & 0xFF);

        return b;
    }

    public static byte SignExtend(in byte value, in int bit) // TODO
    {
        if (bit is < 0 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(bit), bit, null);
        }

        var b = (byte)(value | ~((1 << bit) - 1));

        return b;
    }
}