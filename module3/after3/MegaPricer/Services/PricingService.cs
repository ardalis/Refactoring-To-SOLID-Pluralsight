using System.Data;
using Ardalis.Result;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class PricingService : IPricingService
{
  private readonly IGetUserMarkup _getUserMarkupService;

  public PricingService(IGetUserMarkup getUserMarkupService)
  {
    _getUserMarkupService = getUserMarkupService;
  }

  public Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy)
  {
    if (Context.Session[priceRequest.userName]["PricingOff"] == "Y") return new PriceGroup(0, 0, 0);

    Kitchen kitchen = new Kitchen();
    Order order = new Order();
    Subtotals subtotal = new();
    Part thisPart = new();

    float thisSectionWidth = 0;
    float bbHeight = 0;
    float bbDepth = 0;
    int defaultColor = 0;
    decimal thisUserMarkup = 0;

    bool isIsland = false;
    int wallId = 0;
    float wallHeight = 0;
    DataTable dt = new DataTable();
    DataTable dt2 = new DataTable();
    DataTable dt3 = new DataTable();
    StreamWriter sr = null;

    Context.Session[priceRequest.userName]["WallWeight"] = 0;

    try
    {
      if (priceRequest.wallOrderNum == 0)
      {
        return Result.Forbidden();
      }
      if (priceRequest.kitchenId <= 0)
      {
        return Result.Invalid(new ValidationError("invalid kitchenId"));
      }
      kitchen.GetCustomerKitchen(priceRequest.kitchenId, priceRequest.userName);
      bbHeight = kitchen.BaseHeight;
      bbDepth = kitchen.BaseDepth;
      dt = PopulateWallsDataTable(priceRequest);

      if (dt.Rows.Count == 0)
      {
        return Result.Invalid(new ValidationError("invalid WallOrderNum"));
      }
      priceCalculationStrategy.Create(kitchen);

      defaultColor = Convert.ToInt32(dt.Rows[0]["CabinetColor"]);// dt.Rows[0].Field<int>("CabinetColor");
      wallId = Convert.ToInt32(dt.Rows[0]["WallId"]);
      isIsland = Convert.ToBoolean(dt.Rows[0]["IsIsland"]);
      wallHeight = Convert.ToSingle(dt.Rows[0]["Height"]);

      LoadCabinetsDataTable(wallId, dt2);

      float totalCabinetHeight = 0;
      foreach (DataRow row in dt2.Rows) // each cabinet
      {
        int cabinetId = Convert.ToInt32(row["CabinetId"]);
        thisPart.Width = Convert.ToSingle(row["Width"]); // row.Field<float>("Width");
        thisPart.Depth = Convert.ToSingle(row["Depth"]); // row.Field<float>("Depth");
        thisPart.Height = Convert.ToSingle(row["Height"]); // row.Field<float>("Height");
        thisPart.ColorId = Convert.ToInt32(row["Color"]); // row.Field<int>("Color");
        thisPart.SKU = row.Field<string>("SKU");
        thisPart.Cost = 0;
        thisSectionWidth = 0;
        totalCabinetHeight += thisPart.Height;

        if (!String.IsNullOrEmpty(thisPart.SKU))
        {
          thisPart = GetCostForSku(thisPart);
          thisPart = GetCostForColorChoice(thisPart);
          thisPart.MarkedUpCost = thisPart.Cost * (1 + thisPart.ColorMarkup / 100);
          subtotal.Value += thisPart.MarkedUpCost;
          subtotal.Flat += thisPart.Cost;

          thisUserMarkup = _getUserMarkupService.GetUserMarkup(priceRequest.userName);
          subtotal.Plus += thisPart.MarkedUpCost * (1 + thisUserMarkup / 100);
        }
        priceCalculationStrategy.AddPart(thisPart, thisUserMarkup);

        dt3 = LoadFeatures(cabinetId);
        foreach (DataRow featureRow in dt3.Rows)
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

          if (thisFeature.ColorId > 0)
          {
            thisFeature = LoadFeatureCostInfo(thisUserMarkup, thisFeature);
            subtotal.Value += thisFeature.MarkedUpCost;
            subtotal.Flat += thisFeature.FlatCost;
            subtotal.Plus += thisFeature.UserMarkedUpCost;

            priceCalculationStrategy.AddFeature(thisFeature, thisUserMarkup);
          }
        }
      }

      if (!isIsland)
      {
        float remainingWallHeight = wallHeight - totalCabinetHeight;
        // price wall color backing around cabinets
        if (remainingWallHeight > 0)
        {
          // get width from last cabinet
          var width = LoadWallTreatmentCost(ref thisPart, defaultColor, remainingWallHeight);
          subtotal.Value += thisPart.MarkedUpCost;
          subtotal.Flat += thisPart.Cost;
          subtotal.Plus += thisPart.MarkedUpCost * (1 + thisUserMarkup / 100);

          priceCalculationStrategy.AddWallTreatment(thisPart, thisUserMarkup, remainingWallHeight, width);
        }
      }

      return new PriceGroup(subtotal.Value, subtotal.Flat, subtotal.Plus);
    }
    catch (Exception ex)
    {
      GlobalHelpers.SendErrorEmail("CalcPrice", ex.Message, ex.StackTrace);
      throw;
    }
    finally
    {
      // clean up
      if (sr != null)
      {
        sr.Close();
        sr.Dispose();
      }
    }
  }

  private static float LoadWallTreatmentCost(ref Part thisPart, int defaultColor, float remainingWallHeight)
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

  private static Feature LoadFeatureCostInfo(decimal thisUserMarkup, Feature thisFeature)
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

  private static Part GetCostForColorChoice(Part thisPart)
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

  private static Part GetCostForSku(Part thisPart)
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

  private static void LoadCabinetsDataTable(int wallId, DataTable dt2)
  {
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
          dt2.BeginLoadData();
          dt2.Load(dr);
          dt2.EndLoadData();

        } while (!dr.IsClosed && dr.NextResult());
      }
    }
  }

  private static DataTable PopulateWallsDataTable(PriceRequest priceRequest)
  {
    DataTable dt;
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM Walls WHERE KitchenId = @kitchenId AND WallOrder = @wallOrderNum";
      cmd.Parameters.AddWithValue("@kitchenId", priceRequest.kitchenId);
      cmd.Parameters.AddWithValue("@wallOrderNum", priceRequest.wallOrderNum);
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
