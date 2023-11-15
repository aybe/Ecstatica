using System.IO;

namespace Ecstatica.Tests;

[TestClass]
public sealed class UnitTestImageLowGraph : UnitTestImage
{
    public static IEnumerable<object[]> DecodeLowGraphData()
    {
        return EnumerateFiles(Path.Combine(SourceDirectory, "LOWGRAPH"), "*.RAW");
    }

    [TestMethod]
    [DynamicData(nameof(DecodeLowGraphData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DecodeRawImageName), DynamicDataDisplayNameDeclaringType = typeof(UnitTestImage))]
    public void DecodeLowGraph(string path)
    {
        DecodeRawImage(path);
    }
}