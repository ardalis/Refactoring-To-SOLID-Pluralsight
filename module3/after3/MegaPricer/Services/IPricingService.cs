using Ardalis.Result;

namespace MegaPricer.Services;

public interface IPricingService
{
  Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy);
}
