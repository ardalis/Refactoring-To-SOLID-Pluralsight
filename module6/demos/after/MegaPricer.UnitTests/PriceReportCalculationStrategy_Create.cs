using FluentAssertions;
using MegaPricer.Data;
using MegaPricer.Interfaces;
using MegaPricer.Services;
using NSubstitute;

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
public class NewOrderPriceCalculationStrategy_Create
{
  [Fact]
  public void AssignsKitchenIdToNewOrder()
  {
    var orderDataService = Substitute.For<IOrderDataService>();
    var strategy = new NewOrderPriceCalculationStrategy(orderDataService);

    int testKitchenId = 123456;
    var kitchen = new Kitchen()
    {
      KitchenId = testKitchenId
    };
    Order capturedOrder = null;
    orderDataService.CreateNewOrder(Arg.Do<Order>(order => capturedOrder = order));

    strategy.Create(kitchen);

    capturedOrder.Should().NotBeNull();
    capturedOrder.KitchenId.Should().Be(testKitchenId);
  }

}
