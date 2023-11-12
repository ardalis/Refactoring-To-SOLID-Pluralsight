namespace MegaPricer.Models;

public record struct PriceRequest(int kitchenId,
  int wallOrderNum,
  string userName);
