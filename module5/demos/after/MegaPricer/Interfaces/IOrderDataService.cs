using MegaPricer.Data;
using MegaPricer.Services;
using Feature = MegaPricer.Services.Feature;

namespace MegaPricer.Interfaces;

public interface IOrderDataService
{
  void CreateNewOrder(Order order);
  void InsertOrderItemRecord(Order order, Part thisPart, decimal thisUserMarkup, decimal thisTotalPartCost);
  void AddOrderItem(Order order, decimal thisUserMarkup, Feature thisFeature);
  void AddWallTreatmentToOrder(Order order, Part thisPart, decimal thisUserMarkup);
}
