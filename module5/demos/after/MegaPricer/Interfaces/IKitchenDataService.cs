using MegaPricer.Data;

namespace MegaPricer.Interfaces;

public interface IKitchenDataService
{
  Kitchen GetKitchenByIdAndCustomer(int kitchenId, string userName);
}
