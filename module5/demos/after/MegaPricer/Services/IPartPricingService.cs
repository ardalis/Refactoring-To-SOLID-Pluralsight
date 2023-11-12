namespace MegaPricer.Services;

public interface IPartPricingService
{
  void LoadWallTreatmentCost(Part thisPart, int defaultColor, float remainingWallHeight);
  Feature LoadFeatureCostInfo(decimal thisUserMarkup, Feature thisFeature);
}
