using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Whatever.Extensions;

// ReSharper disable StringLiteralTypo

namespace Ecstatica.Tests;

[TestClass]
public class UnitTest1 : UnitTestBase
{
    private const string SourceDirectory = @"C:\Temp\Ecstatica";

    private static BitmapPalette Palette { get; } = new(Constants.Graphics.GetPalette().Select(s => s.ToColor()).ToList());

    private bool DebugRleBin { get; }

    public static IEnumerable<object[]> DecodeGraphicsData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "GRAPHICS"), "*.RAW");
    }

    public static IEnumerable<object[]> DecodeHiResData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "HIRES"), "*.RAW");
    }

    public static IEnumerable<object[]> DecodeLowGraphData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "LOWGRAPH"), "*.RAW");
    }

    public static IEnumerable<object[]> DecodeViewsData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "VIEWS"), "*.RAW");
    }

    private static IEnumerable<object[]> EnumerateFiles(string path, string searchPattern)
    {
        return Directory.EnumerateFiles(path, searchPattern).Select(s => new object[] { s });
    }

    [TestMethod]
    [DynamicData(nameof(DecodeGraphicsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName))]
    public void DecodeGraphics(string path)
    {
        DecodeRawImage(path);
    }

    [TestMethod]
    [DynamicData(nameof(DecodeHiResData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName))]
    public void DecodeHiRes(string path)
    {
        DecodeRawImage(path);
    }

    [TestMethod]
    [DynamicData(nameof(DecodeLowGraphData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName))]
    public void DecodeLowGraph(string path)
    {
        DecodeRawImage(path);
    }

    [TestMethod]
    [DynamicData(nameof(DecodeViewsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName))]
    public void DecodeViews(string path)
    {
        DecodeRawImage(path);
    }

    public static string DecodeRawImageName(MethodInfo method, object[] data)
    {
        return $"{method.Name} ({Path.GetFileName((string)data[0])})";
    }

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
            ExtractRle(stream);
        }
    }

    private void ExtractRle(FileStream stream)
    {
        var length1 = stream.Read<int>(Endianness.LE);
        var length2 = stream.Read<int>(Endianness.LE);

        var pixels1 = stream.ReadExactly(length1);
        var pixels2 = stream.ReadExactly(length2);

        using var rle1 = DecodeRle1(pixels1);
        using var rle2 = DecodeRle2(pixels2);

        var image = rle1.ToArray();
        var depth = rle2.ToArray();

        {
            GetRleImageDimensions(image.Length, 320, 200, 640, 200, out var pw, out var ph);

            if (DebugRleBin)
            {
                File.WriteAllBytes(new FilePath(stream.Name).AppendToFileName("-image-rle").ChangeExtension(".bin"), image);
            }

            var source = BitmapSource.Create(pw, ph, 96, 96, PixelFormats.Indexed8, Palette, image, pw);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-rle").ChangeExtension(".png"));
        }

        {
            GetRleImageDimensions(depth.Length, 320, 200, 640, 100, out var pw, out var ph);

            if (DebugRleBin)
            {
                File.WriteAllBytes(new FilePath(stream.Name).AppendToFileName("-depth-rle").ChangeExtension(".bin"), depth);
            }

            var source = BitmapSource.Create(pw, ph, 96, 96, PixelFormats.Gray16, null, depth, pw * 2);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth-rle").ChangeExtension(".png"));
        }
    }

    private static void GetRleImageDimensions(long length, int w1, int h1, int w2, int h2, out int pw, out int ph)
    {
        var a1 = Math.Abs(length - 64000);
        var a2 = Math.Abs(length - 128000);

        if (a1 < a2)
        {
            pw = w1;
            ph = h1;
            return;
        }

        if (a2 < a1)
        {
            pw = w2;
            ph = h2;
            return;
        }

        var message = $"Failed to determine image dimensions for length {length}.";

        throw new InvalidOperationException(message);
    }

    private static MemoryStream DecodeRle1(Span<byte> data)
    {
        var maxlen = 128000;
        var bytedata = data;
        var @out = new MemoryStream();
        var pos = 0;
        var lastcolor = 0;

        while (pos < bytedata.Length && @out.Length < maxlen)
        {
            var typeval = bytedata[pos] & 0x03;
            var runlength = bytedata[pos] >> 2;
            pos += 1;
            if (runlength == 0)
            {
                return @out;
            }

            switch (typeval)
            {
                case 0:
                {
                    while (runlength > 0)
                    {
                        var first = bytedata[pos] & 0x0F;
                        var second = bytedata[pos] >> 4;
                        pos += 1;

                        if ((first & 0x08) != 0)
                        {
                            first |= 0xF0;
                        }

                        lastcolor += first;

                        if (lastcolor < 0)
                        {
                            lastcolor += 256;
                        }

                        if (lastcolor > 255)
                        {
                            lastcolor -= 256;
                        }

                        @out.WriteByte(lastcolor.ToByte());

                        runlength -= 1;

                        if (runlength == 0)
                        {
                            break;
                        }

                        if ((second & 0x08) != 0)
                        {
                            second |= 0xF0;
                        }

                        lastcolor += second;

                        if (lastcolor < 0)
                        {
                            lastcolor += 256;
                        }

                        if (lastcolor > 255)
                        {
                            lastcolor -= 256;
                        }

                        @out.WriteByte(lastcolor.ToByte());

                        runlength -= 1;
                    }

                    break;
                }
                case 2:
                {
                    for (var i = 0; i < runlength; i++)
                    {
                        lastcolor = bytedata[pos];
                        @out.WriteByte(lastcolor.ToByte());
                        pos += 1;
                    }

                    break;
                }
                default:
                {
                    lastcolor = bytedata[pos];
                    for (var i = 0; i < runlength; i++)
                    {
                        @out.WriteByte(lastcolor.ToByte());
                    }

                    pos += 1;
                    break;
                }
            }
        }

        return @out;
    }

    private static MemoryStream DecodeRle2(Span<byte> data)
    {
        var maxlen = 128000;
        var bytedata = data;
        var @out = new MemoryStream();
        var pos = 0;
        var lastcolor = 0;
        var writer = new BinaryWriter(@out);
        while (pos < bytedata.Length && @out.Length < maxlen)
        {
            var typeval = bytedata[pos] & 0x03;
            var runlength = bytedata[pos] >> 2;
            pos += 1;
            if (runlength == 0)
            {
                return @out;
            }

            switch (typeval)
            {
                case 0:
                {
                    while (runlength > 0)
                    {
                        var first = bytedata[pos] & 0x0F;
                        var second = bytedata[pos] >> 4;
                        pos += 1;

                        if ((first & 0x08) != 0)
                        {
                            first |= 0xFFF0;
                        }

                        lastcolor += first;
                        lastcolor &= 0xFFFF;

                        writer.Write(lastcolor.ToUInt16());

                        runlength -= 1;

                        if (runlength == 0)
                        {
                            break;
                        }

                        if ((second & 0x08) != 0)
                        {
                            second |= 0xFFF0;
                        }

                        lastcolor += second;
                        lastcolor &= 0xFFFF;

                        writer.Write(lastcolor.ToUInt16());

                        runlength -= 1;
                    }

                    break;
                }
                case 1:
                {
                    for (var i = 0; i < runlength; i++)
                    {
                        int value = bytedata[pos];

                        if ((value & 0x80) != 0)
                        {
                            value |= 0xFF00;
                        }

                        pos += 1;
                        lastcolor += value << 2;
                        lastcolor &= 0xFFFF;
                        writer.Write(lastcolor.ToUInt16());
                    }

                    break;
                }
                case 2:
                {
                    for (var i = 0; i < runlength; i++)
                    {
                        lastcolor = ((bytedata[pos] + (bytedata[pos + 1] << 8)) << 2) & 0xFFFF;
                        writer.Write(lastcolor.ToUInt16());
                        pos += 2;
                    }

                    break;
                }
                default:
                {
                    lastcolor = ((bytedata[pos] + (bytedata[pos + 1] << 8)) << 2) & 0xFFFF;
                    for (var i = 0; i < runlength; i++)
                    {
                        writer.Write(lastcolor.ToUInt16());
                    }

                    pos += 2;
                    break;
                }
            }
        }

        return @out;
    }

    private static void ExtractRaw(FileStream stream)
    {
        var magic = stream.ReadStringAscii(4);

        Assert.AreEqual("wanh", magic);

        var read = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(4, read);

        var pixelWidth = stream.Read<ushort>(Endianness.BE);

// Assert.AreEqual(320, pixelWidth);

        var pixelHeight = stream.Read<ushort>(Endianness.BE);

        // Assert.AreEqual(200, pixelHeight);

        var colorsCount = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(256, colorsCount);

        for (var i = 0; i < 18; i++)
        {
            var b = stream.ReadByte();

            // Assert.AreEqual(0, b, i.ToString());
        }

        var colors = new RGB888[colorsCount];

        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = stream.Read<RGB888>(Endianness.LE);
        }

        var image = stream.ReadExactly(pixelWidth * pixelHeight);

        {
            var palette = new BitmapPalette(colors.Select(s => Color.FromRgb(s.R, s.G, s.B)).ToList());

            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Indexed8, palette, image, pixelWidth);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-raw").ChangeExtension(".png"));
        }

        if (stream.Position == stream.Length)
        {
            return;
        }

        var depth = new ushort[pixelWidth * pixelHeight];

        for (var i = 0; i < depth.Length; i++)
        {
            depth[i] = stream.Read<ushort>(Endianness.LE);
        }

        {
            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Gray16, null, depth, pixelWidth * 2);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth-raw").ChangeExtension(".png"));
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

    private enum RawImageResolution
    {
        Lo,
        Hi
    }
}