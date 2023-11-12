using System.Data;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public partial class PricingService
{
  public class SqlitePricingService : IPricingService
  {
    public Feature LoadFeatureCostInfo(decimal thisUserMarkup, Feature thisFeature)
    {
      using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
      {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM PricingColors WHERE PricingColorId = @pricingColorId";
        cmd.Parameters.AddWithValue("@pricingColorId", thisFeature.ColorId);
        conn.Open();
        using (SqliteDataReader dr = cmd.ExecuteReader())
        {
          if (dr.HasRows && dr.Read())
          {
            thisFeature.ColorName = dr.GetString("Name");
            decimal colorMarkup = dr.GetDecimal("PercentMarkup");
            thisFeature.ColorPerSquareFootCost = dr.GetDecimal("ColorPerSquareFoot");
            thisFeature.WholesalePrice = dr.GetDecimal("WholesalePrice");

            decimal areaInSf = (decimal)(thisFeature.Height * thisFeature.Width / 144);
            thisFeature.FlatCost = areaInSf * thisFeature.ColorPerSquareFootCost;
            if (thisFeature.FlatCost == 0)
            {
              thisFeature.FlatCost = thisFeature.Quantity * thisFeature.WholesalePrice;
            }
            thisFeature.MarkedUpCost = thisFeature.FlatCost * (1 + thisFeature.ColorMarkup / 100);
            thisFeature.UserMarkedUpCost = thisFeature.MarkedUpCost * (1 + thisUserMarkup / 100);
          }
        }
      }

      return thisFeature;
    }

    public float LoadWallTreatmentCost(ref Part thisPart, int defaultColor, float remainingWallHeight)
    {
      float width = thisPart.Width;
      decimal area = (decimal)(remainingWallHeight * width);
      using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
      {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM PricingColors WHERE PricingColorId = @pricingColorId";
        cmd.Parameters.AddWithValue("@pricingColorId", defaultColor);
        conn.Open();
        using (SqliteDataReader dr = cmd.ExecuteReader())
        {
          if (dr.HasRows && dr.Read())
          {
            thisPart.SKU = "PAINT";
            thisPart.ColorName = dr.GetString("Name");
            thisPart.ColorMarkup = dr.GetDecimal("PercentMarkup");
            thisPart.ColorPerSquareFootCost = dr.GetDecimal("ColorPerSquareFoot");

            thisPart.Cost = area * thisPart.ColorPerSquareFootCost / 144;
            thisPart.MarkedUpCost = thisPart.Cost * (1 + thisPart.ColorMarkup / 100);
          }
        }
      }

      return width;
    }
  }
}
