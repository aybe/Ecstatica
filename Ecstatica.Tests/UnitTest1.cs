using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Whatever.Extensions;

// ReSharper disable StringLiteralTypo

namespace Ecstatica.Tests;

[TestClass]
public class UnitTest1
{
    public static IEnumerable<object[]> DecodeRawImageData()
    {
        var files = Directory.GetFiles(@"C:\Temp\Ecstatica\VIEWS", "*.raw");

        foreach (var file in files)
        {
            yield return new object[] { file };
        }
    }

    public static string DecodeRawImageName(MethodInfo method, object[] data)
    {
        return $"{method.Name} ({Path.GetFileName((string)data[0])})";
    }

    [TestMethod]
    [DynamicData(nameof(DecodeRawImageData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName))]
    public void DecodeRawImage(string path)
    {
        using var stream = File.OpenRead(path);

        var type = stream.Read<ushort>(Endianness.BE);

        if (type == 0x6D68) // "mh"
        {
            ExtractRaw(stream);
        }
        else
        {
            Assert.AreEqual(0, type, $"0x{type:X2}");
            Assert.Fail($"0x{type:X2}");
        }
    }

    private static void ExtractRaw(FileStream stream)
    {
        var magic = stream.ReadStringAscii(4);

        Assert.AreEqual("wanh", magic);

        var read = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(4, read);

        var pixelWidth = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(320, pixelWidth);

        var pixelHeight = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(200, pixelHeight);

        var colorsCount = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(256, colorsCount);

        for (var i = 0; i < 18; i++)
        {
            Assert.AreEqual(0, stream.ReadByte(), i.ToString());
        }

        var colors = new RGB888[colorsCount];

        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = stream.Read<RGB888>(Endianness.LE);
        }

        var image = stream.ReadExactly(pixelWidth * pixelHeight);

        var depth = new ushort[pixelWidth * pixelHeight];

        for (var i = 0; i < depth.Length; i++)
        {
            depth[i] = stream.Read<ushort>(Endianness.LE);
        }

        {
            var palette = new BitmapPalette(colors.Select(s => Color.FromRgb(s.R, s.G, s.B)).ToList());

            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Indexed8, palette, image, pixelWidth);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-image").ChangeExtension(".png"));
        }

        {
            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Gray16, null, depth, pixelWidth * 2);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth").ChangeExtension(".png"));
        }
    }

    private static void WritePng(BitmapSource bitmapSource, string path)
    {
        var encoder = new PngBitmapEncoder();

        var frame = BitmapFrame.Create(bitmapSource);

        encoder.Frames.Add(frame);

        using var stream = File.Create(path);

        encoder.Save(stream);
    }
}