using MegaPricer.Models;

namespace MegaPricer.Interfaces;

public interface ICabinetDataService
{
  List<Part> ListCabinetsForWall(int wallId);
}

