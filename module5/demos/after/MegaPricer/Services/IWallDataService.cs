using Ardalis.Result;

namespace MegaPricer.Services;

public interface IWallDataService
{
  Result<Wall> GetWall(int kitchenId, int wallOrderNum);
}

