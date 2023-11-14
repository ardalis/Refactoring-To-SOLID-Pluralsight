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

  [Fact]
  public void ReturnsPriceGroup12_10_18GivenValidData()
  {
    const int flatCost = 10;
    const int colorMarkup = 20;
    const int userMarkup = 50;
    const string sku = "TESTSKU"; // required for subtotal to calculate

    var userMarkupService = Substitute.For<IGetUserMarkup>();
    userMarkupService.GetUserMarkup(_testUserName).Returns(userMarkup);

    var wallDataService = Substitute.For<IWallDataService>();
    wallDataService.GetWall(Arg.Any<int>(), Arg.Any<int>())
      .Returns(new Result<Wall>(new Wall() { WallHeight=36f}));

    var cabinetDataService = Substitute.For<ICabinetDataService>();
    cabinetDataService.ListCabinetsForWall(Arg.Any<int>())
      .Returns(new List<Part>()
      {
        new Part() { 
          Height = 36f, Width = 10f, Depth = 18f,
          SKU = sku,
          Cost = flatCost,
          ColorMarkup = colorMarkup
        },
      });

    var featureDataService = Substitute.For<IFeatureDataService>();
    featureDataService.ListFeaturesForCabinet(Arg.Any<int>())
      .Returns(new List<Feature>());

    var pricingService = new PricingService(
        userMarkupService,
        Substitute.For<IKitchenDataService>(),
        wallDataService,
        cabinetDataService,
        Substitute.For<IPartCostDataService>(),
        featureDataService,
        Substitute.For<IPartPricingService>()
        );
    var priceRequest = GetValidPriceRequest();
    var priceCalculationStrategy = Substitute.For<IPriceCalculationStrategy>();

    var result = pricingService.CalculatePrice(priceRequest, 
      priceCalculationStrategy);

    result.Status.Should().Be(ResultStatus.Ok);
    result.Value.Should().BeEquivalentTo(new PriceGroup(12, flatCost, 18));
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
