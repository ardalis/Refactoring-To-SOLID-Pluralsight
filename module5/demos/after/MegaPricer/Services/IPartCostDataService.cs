namespace MegaPricer.Services;

public interface IPartCostDataService
{
  Part GetCostForSku(Part thisPart);
  Part GetCostForColorChoice(Part thisPart);
}
