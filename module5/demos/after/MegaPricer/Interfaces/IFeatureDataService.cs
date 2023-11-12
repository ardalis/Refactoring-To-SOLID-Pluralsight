using MegaPricer.Services;

namespace MegaPricer.Interfaces;

public interface IFeatureDataService
{
  List<Feature> ListFeaturesForCabinet(int cabinetId);
}
