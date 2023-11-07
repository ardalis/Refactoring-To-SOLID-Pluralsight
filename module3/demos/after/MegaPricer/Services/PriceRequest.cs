namespace MegaPricer.Services;

public record struct PriceRequest(int kitchenId, 
  int wallOrderNum, 
  string userName, 
  string refType);
