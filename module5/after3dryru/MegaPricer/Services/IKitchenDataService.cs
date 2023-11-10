using MegaPricer.Data;

namespace MegaPricer.Services;

public interface IKitchenDataService
{
  Kitchen GetByIdAndCustomer(int kitchenId, string userName);
}
