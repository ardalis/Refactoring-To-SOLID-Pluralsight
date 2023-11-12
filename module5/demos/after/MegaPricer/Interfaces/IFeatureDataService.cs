using MegaPricer.Models;

namespace MegaPricer.Interfaces;

public interface IFeatureDataService
{
  List<Feature> ListFeaturesForCabinet(int cabinetId);
}
