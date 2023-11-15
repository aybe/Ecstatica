using System.IO;
using System.Windows.Media.Imaging;

namespace Ecstatica.Tests;

public static class ImageDecoder
{
    public static void WritePng(BitmapSource bitmapSource, string path)
    {
        var encoder = new PngBitmapEncoder();

        var frame = BitmapFrame.Create(bitmapSource);

        encoder.Frames.Add(frame);

        using var stream = File.Create(path);

        encoder.Save(stream);
    }
}