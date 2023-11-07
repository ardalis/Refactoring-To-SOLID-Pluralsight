using Microsoft.Data.Sqlite;

namespace MegaPricer.Data;

public class Kitchen
{
  public int KitchenId { get; set; }
  public Guid UserId { get; set; }
  public string Name { get; set; }
  public List<Wall> Walls { get; set; } = new();
  public float BaseHeight { get; set; }
  public float BaseDepth { get; set; }

  internal void GetCustomerKitchen(int kitchenId, string userName)
  {
    KitchenId = kitchenId;
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT KitchenId,UserId,Name,BaseHeight,BaseDepth FROM Kitchens WHERE KitchenId = @kitchenId AND UserId = (select UserId from AspNetUsers where UserName=@userName)";
      cmd.Parameters.AddWithValue("@kitchenId", kitchenId);
      cmd.Parameters.AddWithValue("@userName", userName);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        if (dr.HasRows && dr.Read())
        {
          UserId = dr.GetGuid(1);
          Name = dr.GetString(2);
          BaseHeight = dr.GetFloat(3);
          BaseDepth = dr.GetFloat(4);
        }
      }
    }
  }
}
