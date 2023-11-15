using System.IO;

namespace Ecstatica.Tests;

[TestClass]
public sealed class UnitTestImageViews : UnitTestImage
{
    public static IEnumerable<object[]> DecodeViewsData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "VIEWS"), "*.RAW");
    }

    [TestMethod]
    [DynamicData(nameof(DecodeViewsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName), DynamicDataDisplayNameDeclaringType = typeof(UnitTestImage))]
    public void DecodeViews(string path)
    {
        DecodeRawImage(path);
    }
}