using System.IO;

namespace Ecstatica.Tests;

[TestClass]
public sealed class UnitTestImageGraphics : UnitTestImage
{
    public static IEnumerable<object[]> DecodeGraphicsData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "GRAPHICS"), "*.RAW");
    }

    [TestMethod]
    [DynamicData(nameof(DecodeGraphicsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName), DynamicDataDisplayNameDeclaringType = typeof(UnitTestImage))]
    public void DecodeGraphics(string path)
    {
        DecodeRawImage(path);
    }
}