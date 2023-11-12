using Ardalis.Result;
using MegaPricer.Data;

namespace MegaPricer.Services;

public partial class PricingService : IPricingService
{
  private readonly IGetUserMarkup _getUserMarkupService;
  private readonly IKitchenDataService _kitchenDataService;
  private readonly IWallDataService _wallDataService;
  private readonly ICabinetDataService _cabinetDataService;
  private readonly IPartCostDataService _partCostDataService;
  private readonly IFeatureDataService _featureDataService;
  private readonly IPartPricingService _partPricingService;

  public PricingService(IGetUserMarkup getUserMarkupService,
    IKitchenDataService kitchenDataService,
    IWallDataService wallDataService,
    ICabinetDataService cabinetDataService,
    IPartCostDataService partCostDataService,
    IFeatureDataService featureDataService,
    IPartPricingService partPricingService)
  {
    _getUserMarkupService = getUserMarkupService;
    _kitchenDataService = kitchenDataService;
    _wallDataService = wallDataService;
    _cabinetDataService = cabinetDataService;
    _partCostDataService = partCostDataService;
    _featureDataService = featureDataService;
    _partPricingService = partPricingService;
  }

  public Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy)
  {
    Subtotals subtotal = new();

    decimal thisUserMarkup = 0;

    if (priceRequest.wallOrderNum == 0)
    {
      return Result.Forbidden();
    }
    if (priceRequest.kitchenId <= 0)
    {
      return Result.Invalid(new ValidationError("invalid kitchenId"));
    }
    Kitchen kitchen = _kitchenDataService
      .GetKitchenByIdAndCustomer(priceRequest.kitchenId, priceRequest.userName);

    var wallResult = _wallDataService.GetWall(priceRequest.kitchenId, priceRequest.wallOrderNum);
    if (wallResult.Status == ResultStatus.Invalid)
    {
      return Result.Invalid(wallResult.ValidationErrors);
    }
    Wall thisWall = wallResult.Value;
    priceCalculationStrategy.Create(kitchen);

    var cabinets = _cabinetDataService.ListCabinetsForWall(thisWall.WallId);
    float totalCabinetHeight = cabinets.Sum(c => c.Height);
    Part lastPart = new();
    foreach (Part thisPart in cabinets)
    {
      lastPart = thisPart;
      if (!String.IsNullOrEmpty(thisPart.SKU))
      {
        _ = _partCostDataService.GetCostForSku(thisPart);
        _ = _partCostDataService.GetCostForColorChoice(thisPart);
        thisPart.ApplyMarkup(thisPart.ColorMarkup);
        subtotal.Value += thisPart.MarkedUpCost;
        subtotal.Flat += thisPart.Cost;

        thisUserMarkup = _getUserMarkupService.GetUserMarkup(priceRequest.userName);
        subtotal.Plus += thisPart.MarkedUpCost * (1 + thisUserMarkup / 100);
      }
      priceCalculationStrategy.AddPart(thisPart, thisUserMarkup);

      List<Feature> features = _featureDataService
                                .ListFeaturesForCabinet(thisPart.CabinetId);
      foreach (var thisFeature in features)
      {
        if (thisFeature.ColorId > 0)
        {
          _partPricingService.LoadFeatureCostInfo(thisUserMarkup, thisFeature);
          subtotal.Value += thisFeature.MarkedUpCost;
          subtotal.Flat += thisFeature.FlatCost;
          subtotal.Plus += thisFeature.UserMarkedUpCost;

          priceCalculationStrategy.AddFeature(thisFeature, thisUserMarkup);
        }
      }
    }

    if (!thisWall.IsIsland)
    {
      float remainingWallHeight = thisWall.WallHeight - totalCabinetHeight;
      // price wall color backing around cabinets
      if (remainingWallHeight > 0)
      {
        // get width from last cabinet
        _partPricingService.LoadWallTreatmentCost(lastPart, thisWall.DefaultColor, remainingWallHeight);
        subtotal.Value += lastPart.MarkedUpCost;
        subtotal.Flat += lastPart.Cost;
        subtotal.Plus += lastPart.MarkedUpCost * (1 + thisUserMarkup / 100);

        priceCalculationStrategy.AddWallTreatment(lastPart, thisUserMarkup, remainingWallHeight, lastPart.Width);
      }
    }

    return new PriceGroup(subtotal.Value, subtotal.Flat, subtotal.Plus);
  }
}
