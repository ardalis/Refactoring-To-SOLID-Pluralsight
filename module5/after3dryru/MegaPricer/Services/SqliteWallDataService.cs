using System.Data;
using Ardalis.Result;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqliteWallDataService : IWallDataService
{
  public Result<Wall> GetWall(int kitchenId, int wallOrderNum)
  {
    var wallsTable = PopulateWallsDataTable(kitchenId, wallOrderNum);

    if (wallsTable.Rows.Count == 0)
    {
      return Result.Invalid(new ValidationError("invalid WallOrderNum"));
    }

    var wall = new Wall()
    {
      WallId = Convert.ToInt32(wallsTable.Rows[0]["WallId"]),
      DefaultColorId = Convert.ToInt32(wallsTable.Rows[0]["CabinetColor"]),
      IsIsland = Convert.ToBoolean(wallsTable.Rows[0]["IsIsland"]),
      Height = Convert.ToSingle(wallsTable.Rows[0]["Height"])
    };
    
    return wall;
  }

  private DataTable PopulateWallsDataTable(int kitchenId, int wallOrderNum)
  {
    DataTable dt;
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM Walls WHERE KitchenId = @kitchenId AND WallOrder = @wallOrderNum";
      cmd.Parameters.AddWithValue("@kitchenId", kitchenId);
      cmd.Parameters.AddWithValue("@wallOrderNum", wallOrderNum);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        do
        {
          dt = new DataTable();
          dt.BeginLoadData();
          dt.Load(dr);
          dt.EndLoadData();

        } while (!dr.IsClosed && dr.NextResult());
      }
    }

    return dt;
  }

}
