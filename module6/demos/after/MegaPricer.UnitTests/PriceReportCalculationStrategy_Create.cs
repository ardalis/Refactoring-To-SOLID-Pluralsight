using FluentAssertions;
using MegaPricer.Data;
using MegaPricer.Services;

namespace MegaPricer.UnitTests;

public class PriceReportCalculationStrategy_Create
{
  [Fact]
  public void ConfiguresHeadersProperly()
  {
    var strategy = new PriceReportCalculationStrategy();
    var writer = new StringWriter();
    strategy.ReplaceDefaultStreamWriter(writer);

    var kitchen = new Kitchen();
    strategy.Create(kitchen);

    string result = writer.ToString();
    result.Should().Contain("Part Name,Part SKU,Height,Width,Depth,Color,Sq Ft $, Lin Ft $,Per Piece $,# Needed,Part Price,Add On %,Total Part Price");
  }

}
