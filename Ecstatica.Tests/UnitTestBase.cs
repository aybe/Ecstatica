using JetBrains.Annotations;

namespace Ecstatica.Tests;

public abstract class UnitTestBase
{
    [PublicAPI] public required TestContext TestContext { get; set; }

    public void WriteLine(object? value = null)
    {
        TestContext.WriteLine(value?.ToString());
    }
}