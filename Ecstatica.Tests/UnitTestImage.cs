using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Whatever.Extensions;

// ReSharper disable StringLiteralTypo

namespace Ecstatica.Tests;

public class UnitTestImage : UnitTestBase
{
    protected const string SourceDirectory = @"C:\Temp\Ecstatica";

    public static BitmapPalette Palette { get; } = new(Constants.Graphics.GetPalette().Select(s => s.ToColor()).ToList());

    public static bool DebugRleBin { get; } = false;


    protected static IEnumerable<object[]> EnumerateFiles(string path, string searchPattern)
    {
        return Directory.EnumerateFiles(path, searchPattern).Select(s => new object[] { s });
    }


    public static string DecodeRawImageName(MethodInfo method, object[] data)
    {
        return $"{method.Name} ({Path.GetFileName((string)data[0])})";
    }

    protected static void DecodeRawImage(string path)
    {
        using var stream = File.OpenRead(path);

        var type = stream.Read<ushort>(Endianness.BE);

        if (type == 0x6D68) // "mh"
        {
            ImageDecoderRaw.ExtractRaw(stream);
        }
        else
        {
            ImageDecoderRle.ExtractRle(stream);
        }
    }
}