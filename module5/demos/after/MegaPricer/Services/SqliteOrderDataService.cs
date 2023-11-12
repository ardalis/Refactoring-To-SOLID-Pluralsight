using MegaPricer.Data;
using MegaPricer.Interfaces;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqliteOrderDataService : IOrderDataService
{
  public void AddOrderItem(Order order, decimal thisUserMarkup, Feature thisFeature)
  {
    // add this part to the order
    using (var conn2 = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn2.CreateCommand();
      cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
      cmd.Parameters.AddWithValue("@orderId", order.OrderId);
      cmd.Parameters.AddWithValue("@sku", thisFeature.SKU);
      cmd.Parameters.AddWithValue("@quantity", thisFeature.Quantity == 0 ? 1 : thisFeature.Quantity);
      cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(thisFeature.FlatCost));
      cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisFeature.MarkedUpCost - thisFeature.FlatCost));
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisFeature.MarkedUpCost * (1 + thisUserMarkup / 100) - thisFeature.MarkedUpCost));
      conn2.Open();
      cmd.ExecuteNonQuery();
    }
  }

  public void AddWallTreatmentToOrder(Order order, Part thisPart, decimal thisUserMarkup)
  {
    // add this part to the order
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
      cmd.Parameters.AddWithValue("@orderId", order.OrderId);
      cmd.Parameters.AddWithValue("@sku", thisPart.SKU);
      cmd.Parameters.AddWithValue("@quantity", thisPart.Quantity == 0 ? 1 : thisPart.Quantity);
      cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(thisPart.Cost));
      cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisPart.MarkedUpCost - thisPart.Cost));
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisPart.MarkedUpCost * (1 + thisUserMarkup / 100) - thisPart.MarkedUpCost));
      conn.Open();
      cmd.ExecuteNonQuery();
    }
  }

  public void CreateNewOrder(Order order)
  {
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "INSERT INTO ORDERS (KitchenId,OrderDate,OrderStatus,OrderType) VALUES (@kitchenId,@orderDate,@orderStatus,@orderType)";
      cmd.Parameters.AddWithValue("@kitchenId", order.KitchenId);
      cmd.Parameters.AddWithValue("@orderDate", order.OrderDate);
      cmd.Parameters.AddWithValue("@orderStatus", order.OrderStatus);
      cmd.Parameters.AddWithValue("@orderType", order.OrderType);
      conn.Open();
      cmd.ExecuteNonQuery();
      var cmd2 = conn.CreateCommand();
      cmd2.CommandText = "SELECT last_insert_rowid();";
      order.OrderId = Convert.ToInt32(cmd2.ExecuteScalar());
    }
  }

  public void InsertOrderItemRecord(Order order, Part thisPart, decimal thisUserMarkup, decimal thisTotalPartCost)
  {
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
      cmd.Parameters.AddWithValue("@orderId", order.OrderId);
      cmd.Parameters.AddWithValue("@sku", thisPart.SKU);
      cmd.Parameters.AddWithValue("@quantity", thisPart.Quantity == 0 ? 1 : thisPart.Quantity);
      cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(thisPart.Cost));
      cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalPartCost - thisPart.Cost));
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalPartCost * (1 + thisUserMarkup / 100) - thisTotalPartCost));
      conn.Open();
      cmd.ExecuteNonQuery();
    }
  }
}
