namespace MegaPricer.Services;

public interface IFeatureDataService
{
  List<Feature> ListFeaturesForCabinet(int cabinetId);
}
