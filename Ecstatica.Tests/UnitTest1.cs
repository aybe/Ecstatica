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
    public static IEnumerable<object[]> DecodeRawImageData()
    {
        var rle = new[]
        {
            @"C:\Temp\Ecstatica\VIEWS\0001.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0004.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0061.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0071.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0075.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0078.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0094.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0105.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0153.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0154.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0176.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0180.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0182.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0185.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0193.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0203.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0225.RAW",
            @"C:\Temp\Ecstatica\VIEWS\0239.RAW"
        };

        var files = Directory.GetFiles(@"C:\Temp\Ecstatica\VIEWS", "*.raw");

        foreach (var file in files)
        {
            if (rle.Contains(file))
            {
                yield return new object[] { file };
            }
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
            ExtractRle(stream);
        }
    }

    private void ExtractRle(FileStream stream)
    {
        var length1 = stream.Read<int>(Endianness.LE);
        var length2 = stream.Read<int>(Endianness.LE);
        WriteLine(length1);
        WriteLine(length2);

        var pixels1 = stream.ReadExactly(length1);
        var pixels2 = stream.ReadExactly(length2);

        using var rle1 = DecodeRle1(pixels1);

        Assert.AreEqual(64000, rle1.Length);

        using var rle2 = DecodeRle2(pixels2);

        Assert.AreEqual(128000, rle2.Length);

        var pixelWidth = 320;
        var pixelHeight = 200;
        var image = rle1.ToArray();
        var depth = rle2.ToArray();

        var bitmapPalette = new BitmapPalette(Constants.Graphics.GetPalette().Select(s => s.ToColor()).ToList());

        {
            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Indexed8, bitmapPalette, image, pixelWidth);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-rle").ChangeExtension(".png"));
        }

        {
            var source = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Gray16, null, depth, pixelWidth * 2);

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth-rle").ChangeExtension(".png"));
        }
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

            WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-raw").ChangeExtension(".png"));
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
}