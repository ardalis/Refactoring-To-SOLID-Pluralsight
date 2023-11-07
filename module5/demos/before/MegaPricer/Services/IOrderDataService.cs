using MegaPricer.Data;

namespace MegaPricer.Services;

public interface IOrderDataService
{
  void CreateNewOrder(Order order);
  void InsertOrderItemRecord(Order order, Part thisPart, decimal thisUserMarkup, decimal thisTotalPartCost);
  void AddOrderItem(Order order, decimal thisUserMarkup, Feature thisFeature);
  void AddWallTreatmentToOrder(Order order, Part thisPart, decimal thisUserMarkup);
}
