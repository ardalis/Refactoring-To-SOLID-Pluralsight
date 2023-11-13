using MegaPricer.Models;
using FluentAssertions;

namespace MegaPricer.UnitTests;

public class PriceGroup_ToString
{
  [Theory]
  [InlineData(0,0,0,"$0.00|$0.00|$0.00")]
  [InlineData(1,2,3,"$1.00|$2.00|$3.00")]
  [InlineData(1000,1200,1230.33,"$1,000.00|$1,200.00|$1,230.33")]
  public void ReturnsPipeSeparateStringGivenInputs(decimal subtotal,
    decimal flat, decimal plus, string expectedValue)
  {
    string result = new PriceGroup(subtotal, flat, plus).ToString();

    result.Should().Be(expectedValue);
  }
}
