using AwesomeAssertions;

namespace IronHive.Tests.Extensions;

public class DictionaryExtensionsTests
{
    #region GetOrFirstValue

    [Fact]
    public void GetOrFirstValue_KeyGiven_ReturnsThatEntry()
    {
        var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        source.GetOrFirstValue("b").Should().Be(2);
    }

    [Fact]
    public void GetOrFirstValue_KeyGiven_NotRegistered_Throws()
    {
        var source = new Dictionary<string, int> { ["a"] = 1 };

        var act = () => source.GetOrFirstValue("missing");

        act.Should().Throw<KeyNotFoundException>().WithMessage("*missing*");
    }

    [Fact]
    public void GetOrFirstValue_NoKey_ExactlyOneEntry_ReturnsIt()
    {
        var source = new Dictionary<string, int> { ["a"] = 1 };

        source.GetOrFirstValue(null).Should().Be(1);
    }

    [Fact]
    public void GetOrFirstValue_NoKey_EmptySource_Throws()
    {
        var source = new Dictionary<string, int>();

        var act = () => source.GetOrFirstValue(null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Int32*");
    }

    [Fact]
    public void GetOrFirstValue_NoKey_MultipleEntries_Throws()
    {
        var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        var act = () => source.GetOrFirstValue(null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Multiple Int32*").WithMessage("*Specify a provider*");
    }

    #endregion
}
