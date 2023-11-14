using MegaPricer.Data;
using MegaPricer.Services;
using Microsoft.EntityFrameworkCore;

namespace MegaPricer.IntegrationTests;

public class SqliteOrderDataService_CreateNewOrder
{
  public SqliteOrderDataService_CreateNewOrder()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(ConfigurationSettings.ConnectionString)
        .Options;
    var context = new ApplicationDbContext(options);
    context.Database.EnsureCreated();
  }

  [Fact]
  public void AddsNewOrderToDatabase()
  {
    string testStatus = Guid.NewGuid().ToString();
    var order = new Order()
    {
      KitchenId = 1,
      OrderDate = DateTime.Now,
      OrderStatus = testStatus,
      OrderType = "test"
    };

    var dataService = new SqliteOrderDataService();
    dataService.CreateNewOrder(order);
    
    Assert.True(OrderExists(order.OrderId));
  }

  private bool OrderExists(int id)
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(ConfigurationSettings.ConnectionString)
        .Options;
    var context = new ApplicationDbContext(options);
    return context.Orders.Any(e => e.OrderId == id);
  }
}
