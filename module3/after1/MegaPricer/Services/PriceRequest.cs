namespace MegaPricer.Services;

public record struct PriceRequest(int KitchenId,
                                  int WallOrderNum,
                                  string UserName,
                                  string RefType);
