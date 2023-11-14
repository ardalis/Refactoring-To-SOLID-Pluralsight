using MegaPricer.Models;
using FluentAssertions;
using MegaPricer.Interfaces;
using NSubstitute;
using MegaPricer.Services;
using Ardalis.Result;

namespace MegaPricer.UnitTests;

public class PricingService_CalculatePrice
{
  private readonly string _testUserName = "test";

  [Fact]
  public void ReturnsInvalidGivenInvalidWallResult()
  {
    var wallDataService = Substitute.For<IWallDataService>();
    wallDataService.GetWall(Arg.Any<int>(), Arg.Any<int>())
      .Returns(Result.Invalid(new ValidationError("invalid wall")));

    var pricingService = new PricingService(
        Substitute.For<IGetUserMarkup>(),
        Substitute.For<IKitchenDataService>(),
        wallDataService,
        Substitute.For<ICabinetDataService>(),
        Substitute.For<IPartCostDataService>(),
        Substitute.For<IFeatureDataService>(),
        Substitute.For<IPartPricingService>()
        );
    var priceRequest = GetValidPriceRequest();
    var priceCalculationStrategy = Substitute.For<IPriceCalculationStrategy>();

    var result = pricingService.CalculatePrice(priceRequest, priceCalculationStrategy);

    result.Status.Should().Be(ResultStatus.Invalid);
    result.ValidationErrors.First().Should().BeEquivalentTo(new ValidationError("invalid wall"));
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

}
