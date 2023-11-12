using System.Data;
using MegaPricer.Data;
using MegaPricer.Interfaces;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqliteCabinetDataService : ICabinetDataService
{
  public List<Part> ListCabinetsForWall(int wallId)
  {
    var dt = LoadCabinetsDataTable(wallId);

    var cabinets = new List<Part>();

    foreach (DataRow row in dt.Rows) // each cabinet
    {
      Part thisPart = new();
      thisPart.CabinetId = Convert.ToInt32(row["CabinetId"]);
      thisPart.Width = Convert.ToSingle(row["Width"]);
      thisPart.Depth = Convert.ToSingle(row["Depth"]);
      thisPart.Height = Convert.ToSingle(row["Height"]);
      thisPart.ColorId = Convert.ToInt32(row["Color"]);
      thisPart.SKU = row.Field<string>("SKU");
      thisPart.Cost = 0;

      cabinets.Add(thisPart);
    }
    return cabinets;
  }

  private static DataTable LoadCabinetsDataTable(int wallId)
  {
    var dt = new DataTable();
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM Cabinets WHERE WallId = @wallId ORDER BY CabinetOrder";
      cmd.Parameters.AddWithValue("@wallId", wallId);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        do
        {
          dt.BeginLoadData();
          dt.Load(dr);
          dt.EndLoadData();

        } while (!dr.IsClosed && dr.NextResult());
      }
    }
    return dt;
  }

}

