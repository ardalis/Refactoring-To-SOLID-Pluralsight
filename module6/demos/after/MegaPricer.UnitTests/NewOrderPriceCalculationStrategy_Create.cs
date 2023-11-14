using FluentAssertions;
using MegaPricer.Data;
using MegaPricer.Interfaces;
using MegaPricer.Services;
using NSubstitute;

namespace MegaPricer.UnitTests;

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
