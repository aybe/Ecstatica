using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Whatever.Extensions;

namespace Ecstatica.Tests;

public static class ImageDecoderRaw
{
    public static void ExtractRaw(FileStream stream)
    {
        var magic = stream.ReadStringAscii(4);

        Assert.AreEqual("wanh", magic); // TODO

        var unknown = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(4, unknown);

        var pw = stream.Read<ushort>(Endianness.BE);
        var ph = stream.Read<ushort>(Endianness.BE);
        var cc = stream.Read<ushort>(Endianness.BE);

        Assert.AreEqual(256, cc);

        for (var i = 0; i < 18; i++)
        {
            var b = stream.ReadByte(); // TODO not always 0
        }

        var colors = new RGB888[cc];

        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = stream.Read<RGB888>(Endianness.LE);
        }

        var image = stream.ReadExactly(pw * ph);

        {
            var palette = new BitmapPalette(colors.Select(s => Color.FromRgb(s.R, s.G, s.B)).ToList());

            var source = BitmapSource.Create(pw, ph, 96, 96, PixelFormats.Indexed8, palette, image, pw);

            ImageDecoder.WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-raw").ChangeExtension(".png"));
        }

        if (stream.Position == stream.Length)
        {
            return; // TODO clarify
        }

        var depth = new ushort[pw * ph];

        for (var i = 0; i < depth.Length; i++)
        {
            depth[i] = stream.Read<ushort>(Endianness.LE);
        }

        {
            var source = BitmapSource.Create(pw, ph, 96, 96, PixelFormats.Gray16, null, depth, pw * 2);

            ImageDecoder.WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth-raw").ChangeExtension(".png"));
        }
    }
}