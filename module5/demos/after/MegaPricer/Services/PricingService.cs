using System.Data;
using Ardalis.Result;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class PricingService : IPricingService
{
  private readonly IGetUserMarkup _getUserMarkupService;
  private readonly IKitchenDataService _kitchenDataService;
  private readonly IWallDataService _wallDataService;
  private readonly ICabinetDataService _cabinetDataService;
  private readonly IPartCostDataService _partCostDataService;

  public PricingService(IGetUserMarkup getUserMarkupService,
    IKitchenDataService kitchenDataService,
    IWallDataService wallDataService,
    ICabinetDataService cabinetDataService,
    IPartCostDataService partCostDataService)
  {
    _getUserMarkupService = getUserMarkupService;
    _kitchenDataService = kitchenDataService;
    _wallDataService = wallDataService;
    _cabinetDataService = cabinetDataService;
    _partCostDataService = partCostDataService;
  }

  public Result<PriceGroup> CalculatePrice(PriceRequest priceRequest,
    IPriceCalculationStrategy priceCalculationStrategy)
  {
    Subtotals subtotal = new();

    decimal thisUserMarkup = 0;

    if (priceRequest.wallOrderNum == 0)
    {
      return Result.Forbidden();
    }
    if (priceRequest.kitchenId <= 0)
    {
      return Result.Invalid(new ValidationError("invalid kitchenId"));
    }
    Kitchen kitchen = _kitchenDataService
      .GetKitchenByIdAndCustomer(priceRequest.kitchenId, priceRequest.userName);

    var wallResult = _wallDataService.GetWall(priceRequest.kitchenId, priceRequest.wallOrderNum);
    if (wallResult.Status == ResultStatus.Invalid)
    {
      return Result.Invalid(wallResult.ValidationErrors);
    }
    Wall thisWall = wallResult.Value;
    priceCalculationStrategy.Create(kitchen);

    var cabinets = _cabinetDataService.ListCabinetsForWall(thisWall.WallId);
    float totalCabinetHeight = cabinets.Sum(c => c.Height);
    Part lastPart = new();
    foreach (Part thisPart in cabinets)
    {
      lastPart = thisPart;
      if (!String.IsNullOrEmpty(thisPart.SKU))
      {
        _ = _partCostDataService.GetCostForSku(thisPart);
        _ = _partCostDataService.GetCostForColorChoice(thisPart);
        thisPart.ApplyMarkup(thisPart.ColorMarkup);
        subtotal.Value += thisPart.MarkedUpCost;
        subtotal.Flat += thisPart.Cost;

        thisUserMarkup = _getUserMarkupService.GetUserMarkup(priceRequest.userName);
        subtotal.Plus += thisPart.MarkedUpCost * (1 + thisUserMarkup / 100);
      }
      priceCalculationStrategy.AddPart(thisPart, thisUserMarkup);

      DataTable featuresDataTable = LoadFeatures(thisPart.CabinetId);
      foreach (DataRow featureRow in featuresDataTable.Rows)
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

    if (!thisWall.IsIsland)
    {
      float remainingWallHeight = thisWall.WallHeight - totalCabinetHeight;
      // price wall color backing around cabinets
      if (remainingWallHeight > 0)
      {
        // get width from last cabinet
        var width = LoadWallTreatmentCost(ref lastPart, thisWall.DefaultColor, remainingWallHeight);
        subtotal.Value += lastPart.MarkedUpCost;
        subtotal.Flat += lastPart.Cost;
        subtotal.Plus += lastPart.MarkedUpCost * (1 + thisUserMarkup / 100);

        priceCalculationStrategy.AddWallTreatment(lastPart, thisUserMarkup, remainingWallHeight, width);
      }
    }

    return new PriceGroup(subtotal.Value, subtotal.Flat, subtotal.Plus);
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

}
