using Ardalis.Result;
using MegaPricer.Services;

namespace MegaPricer.Interfaces;

public interface IWallDataService
{
  Result<Wall> GetWall(int kitchenId, int wallOrderNum);
}

