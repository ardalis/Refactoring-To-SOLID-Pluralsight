using System.Data;
using MegaPricer.Data;
using MegaPricer.Interfaces;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqliteFeatureDataService : IFeatureDataService
{
  public List<Feature> ListFeaturesForCabinet(int cabinetId)
  {
    var dt = LoadFeatures(cabinetId);
    var features = new List<Feature>();
    foreach (DataRow featureRow in dt.Rows)
    {
      var thisFeature = new Feature()
      {
        FeatureId = Convert.ToInt32(featureRow["FeatureId"]),
        ColorId = Convert.ToInt32(featureRow["Color"]),
        SKU = Convert.ToString(featureRow["SKU"]),
        Quantity = Convert.ToInt32(featureRow["Quantity"]),
        Height = Convert.ToSingle(featureRow["Height"]),
        Width = Convert.ToSingle(featureRow["Width"])
      };
      features.Add(thisFeature);
    }
    return features;
  }

  private static DataTable LoadFeatures(int cabinetId)
  {
    DataTable dt3;
    // get feature cost
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM Features WHERE CabinetId = @cabinetId ORDER BY FeatureOrder";
      cmd.Parameters.AddWithValue("@cabinetId", cabinetId);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        do
        {
          dt3 = new DataTable();
          dt3.BeginLoadData();
          dt3.Load(dr);
          dt3.EndLoadData();

        } while (!dr.IsClosed && dr.NextResult());
      }
    }

    return dt3;
  }

}
