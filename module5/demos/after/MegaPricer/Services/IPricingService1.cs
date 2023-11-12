namespace MegaPricer.Services;

public partial class PricingService
{
  public interface IPricingService
  {
    float LoadWallTreatmentCost(ref Part thisPart, int defaultColor, float remainingWallHeight);
    Feature LoadFeatureCostInfo(decimal thisUserMarkup, Feature thisFeature);
  }
}
