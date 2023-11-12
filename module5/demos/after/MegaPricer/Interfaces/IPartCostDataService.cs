using MegaPricer.Models;

namespace MegaPricer.Interfaces;

public interface IPartCostDataService
{
  Part GetCostForSku(Part thisPart);
  Part GetCostForColorChoice(Part thisPart);
}
