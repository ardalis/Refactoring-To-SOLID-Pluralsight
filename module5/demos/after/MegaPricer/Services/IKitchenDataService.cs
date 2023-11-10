using MegaPricer.Data;

namespace MegaPricer.Services;

public interface IKitchenDataService
{
  Kitchen GetKitchenByIdAndCustomer(int kitchenId, string userName);
}
