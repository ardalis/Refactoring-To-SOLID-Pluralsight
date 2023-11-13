using Ardalis.Result;
using MegaPricer.Models;

namespace MegaPricer.Interfaces;

public interface IPricingService
{
  Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy);
}
