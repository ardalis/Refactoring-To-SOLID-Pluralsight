using Ardalis.Result;
using MegaPricer.Data;

namespace MegaPricer.Services;

public class PricingServiceDecorator : IPricingService
{
  private readonly PricingService _pricingService;

  public PricingServiceDecorator(PricingService pricingService)
  {
    _pricingService = pricingService;
  }

  public Result<PriceGroup> CalculatePrice(PriceRequest priceRequest, IPriceCalculationStrategy priceCalculationStrategy)
  {
    if (Context.Session[priceRequest.userName]["PricingOff"] == "Y") return new PriceGroup(0, 0, 0);

    Context.Session[priceRequest.userName]["WallWeight"] = 0;

    try
    {
      return _pricingService.CalculatePrice(priceRequest, priceCalculationStrategy);
    }
    catch (Exception ex)
    {
      GlobalHelpers.SendErrorEmail("CalcPrice", ex.Message, ex.StackTrace);
      throw;
    }
  }
}
