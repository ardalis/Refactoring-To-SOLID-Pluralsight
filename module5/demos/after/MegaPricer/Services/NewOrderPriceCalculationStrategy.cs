using MegaPricer.Data;
using MegaPricer.Interfaces;

namespace MegaPricer.Services;

public class NewOrderPriceCalculationStrategy : IPriceCalculationStrategy
{
  private Order _newOrder = new();
  private readonly IOrderDataService _orderDataService;

  public NewOrderPriceCalculationStrategy(IOrderDataService orderDataService)
  {
    _orderDataService = orderDataService;
  }

  public void Create(Kitchen kitchen)
  {
    // create a new order
    _newOrder.KitchenId = kitchen.KitchenId;
    _orderDataService.CreateNewOrder(_newOrder);
  }

  public void AddPart(Part part, decimal userMarkup)
  {
    // add this part to the order
    _orderDataService.InsertOrderItemRecord(_newOrder, part, userMarkup, part.MarkedUpCost);
  }

  public void AddFeature(Feature feature, decimal userMarkup)
  {
    _orderDataService.AddOrderItem(_newOrder, userMarkup, feature);
  }

  public void AddWallTreatment(Part part, decimal userMarkup, float remainingWallHeight, float width)
  {
    _orderDataService.AddWallTreatmentToOrder(_newOrder, part, userMarkup);
  }
}
