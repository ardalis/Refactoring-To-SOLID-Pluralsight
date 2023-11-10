using System.Data;
using Ardalis.Result;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqliteWallDataService : IWallDataService
{
  public Result<Wall> GetWall(int kitchenId, int wallOrderNum)
  {
    var wallTable = PopulateWallsDataTable(kitchenId, wallOrderNum);

    if (wallTable.Rows.Count == 0)
    {
      return Result.Invalid(new ValidationError("invalid WallOrderNum"));
    }
    var thisWall = new Wall()
    {
      WallId = Convert.ToInt32(wallTable.Rows[0]["WallId"]),
      DefaultColor = Convert.ToInt32(wallTable.Rows[0]["CabinetColor"]),
      IsIsland = Convert.ToBoolean(wallTable.Rows[0]["IsIsland"]),
      WallHeight = Convert.ToSingle(wallTable.Rows[0]["Height"])
    };
    return thisWall;
  }

  private static DataTable PopulateWallsDataTable(int kitchenId, int wallOrderNum)
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

