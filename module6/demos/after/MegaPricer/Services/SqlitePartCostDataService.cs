using System.Data;
using MegaPricer.Data;
using MegaPricer.Interfaces;
using MegaPricer.Models;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class SqlitePartCostDataService : IPartCostDataService
{
  public Part GetCostForColorChoice(Part thisPart)
  {
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM PricingColors WHERE PricingColorId = @pricingColorId";
      cmd.Parameters.AddWithValue("@pricingColorId", thisPart.ColorId);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        if (dr.HasRows && dr.Read())
        {
          thisPart.ColorName = dr.GetString("Name");
          thisPart.ColorMarkup = dr.GetDecimal("PercentMarkup");
          thisPart.ColorPerSquareFootCost = dr.GetDecimal("ColorPerSquareFoot");
        }
      }
    }

    return thisPart;
  }

  public Part GetCostForSku(Part thisPart)
  {
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM PricingSkus WHERE SKU = @sku";
      cmd.Parameters.AddWithValue("@sku", thisPart.SKU);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        if (dr.HasRows && dr.Read())
        {
          thisPart.Cost = dr.GetDecimal("WholesalePrice");
        }
      }
    }

    return thisPart;
  }
}
