using HrPlatform.Utils;
using Xunit;

namespace HrPlatform.Tests.Utils;

public class USStatesTests
{
    [Theory]
    [InlineData("CA", "California")]
    [InlineData("NY", "New York")]
    [InlineData("TX", "Texas")]
    [InlineData("FL", "Florida")]
    [InlineData("DC", "District of Columbia")]
    [InlineData("WY", "Wyoming")]
    public void GetStateName_ValidAbbreviation_ReturnsStateName(string abbreviation, string expectedName)
    {
        var result = USStates.GetStateName(abbreviation);
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData("ca", "California")]
    [InlineData("Tx", "Texas")]
    [InlineData("fl", "Florida")]
    [InlineData("ny", "New York")]
    public void GetStateName_CaseInsensitiveAbbreviation_ReturnsStateName(string abbreviation, string expectedName)
    {
        var result = USStates.GetStateName(abbreviation);
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData("  CA  ", "California")]
    [InlineData(" TX ", "Texas")]
    [InlineData("\tIL\n", "Illinois")]
    public void GetStateName_WhitespacePaddedAbbreviation_ReturnsTrimmedMatch(string abbreviation, string expectedName)
    {
        var result = USStates.GetStateName(abbreviation);
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void GetStateName_NullOrWhitespace_ReturnsEmptyString(string? input)
    {
        var result = USStates.GetStateName(input);
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("ZZ", "ZZ")]
    [InlineData("Unknown", "Unknown")]
    [InlineData("  Puerto Rico  ", "Puerto Rico")]
    public void GetStateName_UnknownCode_ReturnsTrimmedInput(string input, string expected)
    {
        var result = USStates.GetStateName(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void StatesDictionary_Contains50StatesAndDC()
    {
        Assert.Equal(51, USStates.States.Count);
        Assert.True(USStates.States.ContainsKey("dc"));
        Assert.True(USStates.States.ContainsKey("DC"));
    }
}
