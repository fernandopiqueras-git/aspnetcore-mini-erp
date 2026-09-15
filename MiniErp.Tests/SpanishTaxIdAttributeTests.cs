using MiniErp.Models;

namespace MiniErp.Tests;

public class SpanishTaxIdAttributeTests
{
    [Theory]
    [InlineData("12345678Z")]
    [InlineData("X2482300W")]
    [InlineData("B45000007")]
    public void IsValid_AcceptsValidIdentifiers(string value)
    {
        Assert.True(new SpanishTaxIdAttribute().IsValid(value));
    }

    [Theory]
    [InlineData("12345678A")]
    [InlineData("B45000001")]
    [InlineData("incorrecto")]
    public void IsValid_RejectsInvalidIdentifiers(string value)
    {
        Assert.False(new SpanishTaxIdAttribute().IsValid(value));
    }
}
