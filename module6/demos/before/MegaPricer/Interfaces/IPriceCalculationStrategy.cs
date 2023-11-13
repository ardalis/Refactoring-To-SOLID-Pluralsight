using MegaPricer.Data;
using MegaPricer.Models;
using Feature = MegaPricer.Models.Feature;

namespace MegaPricer.Interfaces;

public interface IPriceCalculationStrategy
{
  void Create(Kitchen kitchen);
  void AddPart(Part part, decimal userMarkup);
  void AddFeature(Feature feature, decimal userMarkup);
  void AddWallTreatment(Part part, decimal userMarkup, float remainingWallHeight, float width);
}
