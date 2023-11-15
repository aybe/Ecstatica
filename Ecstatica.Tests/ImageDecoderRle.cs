using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Whatever.Extensions;

namespace Ecstatica.Tests;

public static class ImageDecoderRle
{
    private static bool DebugRleBin { get; } = false;

    private static (int W, int H) GetRleImageSize(int length)
    {
        return length switch
        {
            128000 => (320, 200),
            614400 => (640, 480),
            _      => throw new NotSupportedException(length.ToString())
        };
    }

    private static MemoryStream DecodeRle1(Span<byte> data)
    {
        var bytedata = data;
        var @out = new MemoryStream();
        var pos = 0;
        var lastcolor = 0;

        while (pos < bytedata.Length)
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
        var bytedata = data;
        var @out = new MemoryStream();
        var pos = 0;
        var lastcolor = 0;
        var writer = new BinaryWriter(@out);

        while (pos < bytedata.Length)
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

    public static void ExtractRle(FileStream stream)
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
            var (w, h) = GetRleImageSize(depth.Length);

            if (DebugRleBin)
            {
                File.WriteAllBytes(new FilePath(stream.Name).AppendToFileName("-image-rle").ChangeExtension(".bin"), image);
            }

            var source = BitmapSource.Create(w, h, 96, 96, PixelFormats.Indexed8, UnitTestImage.Palette, image, w);

            ImageDecoder.WritePng(source, new FilePath(stream.Name).AppendToFileName("-image-rle").ChangeExtension(".png"));
        }

        {
            var (w, h) = GetRleImageSize(depth.Length);

            if (DebugRleBin)
            {
                File.WriteAllBytes(new FilePath(stream.Name).AppendToFileName("-depth-rle").ChangeExtension(".bin"), depth);
            }

            var source = BitmapSource.Create(w, h, 96, 96, PixelFormats.Gray16, null, depth, w * 2);

            ImageDecoder.WritePng(source, new FilePath(stream.Name).AppendToFileName("-depth-rle").ChangeExtension(".png"));
        }
    }
}