using System.IO;

namespace Ecstatica.Tests;

[TestClass]
public sealed class UnitTestImageHiRes : UnitTestImage
{
    public static IEnumerable<object[]> DecodeHiResData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "HIRES"), "*.RAW");
    }

    [TestMethod]
    [DynamicData(nameof(DecodeHiResData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName), DynamicDataDisplayNameDeclaringType = typeof(UnitTestImage))]
    public void DecodeHiRes(string path)
    {
        DecodeRawImage(path);
    }
}