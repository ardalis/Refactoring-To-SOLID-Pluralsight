using Ardalis.Result;
using MegaPricer.Services;

namespace MegaPricer.Interfaces;

public interface IPricingService
{
  Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy);
}
