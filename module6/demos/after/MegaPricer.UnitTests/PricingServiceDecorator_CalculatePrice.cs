using MegaPricer.Models;
using FluentAssertions;
using MegaPricer.Interfaces;
using NSubstitute;
using MegaPricer.Services;
using MegaPricer.Data;
using Ardalis.Result;

namespace MegaPricer.UnitTests;
public class PricingServiceDecorator_CalculatePrice
{
  private readonly string _testUserName = "test";

  public PricingServiceDecorator_CalculatePrice()
  {
    Context.Session.Clear();
    Context.Session.Add(_testUserName, new Dictionary<string, object>());
  }

  [Fact]
  public void ReturnsZeroGivenPricingOff()
  {
    var pricingService = GetPricingService();
    var decorator = new PricingServiceDecorator(pricingService);
    var priceRequest = GetValidPriceRequest();
    var priceCalculationStrategy = Substitute.For<IPriceCalculationStrategy>();
    Context.Session[_testUserName]["PricingOff"] = "Y";

    var result = decorator.CalculatePrice(priceRequest, priceCalculationStrategy);

    result.Value.Should().BeEquivalentTo(new PriceGroup(0, 0, 0));
  }

  [Fact]
  public void ReturnsInvalidGivenWallOrderNumZero()
  {
    var pricingService = GetPricingService();
    var decorator = new PricingServiceDecorator(pricingService);
    var priceRequest = GetValidPriceRequest();
    priceRequest.wallOrderNum = 0;
    var priceCalculationStrategy = Substitute.For<IPriceCalculationStrategy>();
    Context.Session[_testUserName]["PricingOff"] = "N";

    var result = decorator.CalculatePrice(priceRequest, priceCalculationStrategy);

    result.Status.Should().Be(ResultStatus.Forbidden);
  }

  [Fact]
  public void ReturnsInvalidGivenKitchenIdZero()
  {
    var pricingService = GetPricingService();
    var decorator = new PricingServiceDecorator(pricingService);
    var priceRequest = GetValidPriceRequest();
    priceRequest.kitchenId = 0;
    var priceCalculationStrategy = Substitute.For<IPriceCalculationStrategy>();
    Context.Session[_testUserName]["PricingOff"] = "N";

    var result = decorator.CalculatePrice(priceRequest, priceCalculationStrategy);

    result.Status.Should().Be(ResultStatus.Invalid);
    result.ValidationErrors.First().Should()
      .BeEquivalentTo(new ValidationError("invalid kitchenId"));
  }

  private PriceRequest GetValidPriceRequest()
  {
    return new PriceRequest()
    {
      userName = _testUserName,
      wallOrderNum = 1,
      kitchenId = 1
    };
  }

  private PricingService GetPricingService()
  {
    return new PricingService(
        Substitute.For<IGetUserMarkup>(),
        Substitute.For<IKitchenDataService>(),
        Substitute.For<IWallDataService>(),
        Substitute.For<ICabinetDataService>(),
        Substitute.For<IPartCostDataService>(),
        Substitute.For<IFeatureDataService>(),
        Substitute.For<IPartPricingService>()
        );
  }
}
