using Ardalis.Result;
using MegaPricer.Models;

namespace MegaPricer.Interfaces;

public interface IWallDataService
{
  Result<Wall> GetWall(int kitchenId, int wallOrderNum);
}

