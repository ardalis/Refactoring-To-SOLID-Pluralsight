namespace MegaPricer.Services;

public interface ICabinetDataService
{
  List<Part> ListCabinetsForWall(int wallId);
}

