using System.Data;
using Ardalis.Result;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;
public class PricingService
{
  public Result<PriceGroup> CalculatePrice(PriceRequest priceRequest)
  {
    if (Context.Session[priceRequest.UserName]["PricingOff"] == "Y") return new PriceGroup(0, 0, 0);

    Kitchen kitchen = new Kitchen();
    Order order = new Order();
    Subtotals subtotals = new();
    float thisSectionWidth = 0;
    float bbHeight = 0;
    float bbDepth = 0;
    int defaultColor = 0;
    decimal thisUserMarkup = 0;
    decimal thisTotalPartCost = 0;
    bool isIsland = false;
    int wallId = 0;
    float wallHeight = 0;
    Part thisPart = new();
    DataTable dt = new DataTable();
    DataTable dt2 = new DataTable();
    DataTable dt3 = new DataTable();
    StreamWriter sr = null;

    Context.Session[priceRequest.UserName]["WallWeight"] = 0;

    try
    {
      if (priceRequest.WallOrderNum == 0)
      {
        return Result.Error("Session expired: Log in again.");
      }
      if (priceRequest.KitchenId <= 0)
      {
        return Result.Invalid(new ValidationError("invalid kitchenId"));
      }
      kitchen.GetCustomerKitchen(priceRequest.KitchenId, priceRequest.UserName);
      bbHeight = kitchen.BaseHeight;
      bbDepth = kitchen.BaseDepth;
      dt = LoadWallsDataTable(priceRequest);

      if (dt.Rows.Count == 0)
      {
        return Result.Invalid(new ValidationError("invalid wallOrderNum"));
      }

      if (priceRequest.RefType == "PriceReport")
      {
        // Start writing to the report file
        string baseDirectory = AppContext.BaseDirectory;
        string path = baseDirectory + "Orders.csv";
        sr = new StreamWriter(path);
        sr.WriteLine($"{kitchen.Name} ({kitchen.KitchenId}) - Run time: {DateTime.Now.ToLongTimeString()} ");
        sr.WriteLine("");
        sr.WriteLine("Part Name,Part SKU,Height,Width,Depth,Color,Sq Ft $, Lin Ft $,Per Piece $,# Needed,Part Price,Add On %,Total Part Price");
      }
      else if (priceRequest.RefType == "Order")
      {
        CreateNewOrder(priceRequest, order);
      }

      defaultColor = Convert.ToInt32(dt.Rows[0]["CabinetColor"]);// dt.Rows[0].Field<int>("CabinetColor");
      wallId = Convert.ToInt32(dt.Rows[0]["WallId"]);
      isIsland = Convert.ToBoolean(dt.Rows[0]["IsIsland"]);
      wallHeight = Convert.ToSingle(dt.Rows[0]["Height"]);

      LoadCabinetsDataTable(wallId, dt2);

      float totalCabinetHeight = 0;
      foreach (DataRow row in dt2.Rows) // each cabinet
      {
        int cabinetId = Convert.ToInt32(row["CabinetId"]);
        thisPart = new Part()
        {
          Width = Convert.ToSingle(row["Width"]),
          Depth = Convert.ToSingle(row["Depth"]),
          Height = Convert.ToSingle(row["Height"]),
          ColorId = Convert.ToInt32(row["Color"]),
          SKU = row.Field<string>("SKU")
        };
        thisSectionWidth = 0;
        totalCabinetHeight += thisPart.Height;

        if (!String.IsNullOrEmpty(thisPart.SKU))
        {
          thisPart.Cost = GetCostForSku(thisPart.SKU);
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
          thisTotalPartCost = thisPart.Cost * (1 + thisPart.ColorMarkup / 100);
          subtotals.Value += thisTotalPartCost;
          subtotals.Flat += thisPart.Cost;

          using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
          {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM UserMarkups WHERE UserName = @userName";
            cmd.Parameters.AddWithValue("@userName", priceRequest.UserName);
            conn.Open();
            using (SqliteDataReader dr = cmd.ExecuteReader())
            {
              if (dr.HasRows && dr.Read())
              {
                thisUserMarkup = dr.GetDecimal("MarkupPercent");
              }
            }
          }
          subtotals.Plus += thisTotalPartCost * (1 + thisUserMarkup / 100);
        }

        if (priceRequest.RefType == "Order")
        {
          AddOrderItem(order, thisUserMarkup, thisTotalPartCost, thisPart);
        }
        else if (priceRequest.RefType == "PriceReport")
        {
          // write out required part(s) to the report file
          sr.WriteLine($"{thisPart.SKU},{thisPart.Height},{thisPart.Width},{thisPart.Depth},{thisPart.ColorName},{thisPart.ColorPerSquareFootCost},{thisPart.LinearFootCost},{thisPart.Cost},{thisPart.Quantity},{thisPart.Cost * thisPart.Quantity},{thisPart.ColorMarkup},{GlobalHelpers.Format(thisTotalPartCost)}");
        }
        else
        {
          // Just get the cost
        }

        dt3 = LoadFeatureDataTable(cabinetId);
        foreach (DataRow featureRow in dt3.Rows)
        {
          var thisFeature = new Feature()
          {
            FeatureId = Convert.ToInt32(featureRow["FeatureId"]),
            ColorId = Convert.ToInt32(featureRow["Color"]),
            SKU = Convert.ToString(featureRow["SKU"]),
            Quantity = Convert.ToInt32(featureRow["Quantity"]),
            Height = Convert.ToSingle(featureRow["Height"]),
            Width = Convert.ToSingle(featureRow["Width"]),
          };
          decimal thisTotalFeatureCost = 0;

          if (thisFeature.ColorId > 0)
          {
            LoadFeatureCostInfo(thisUserMarkup, thisFeature);
            subtotals.Value += thisFeature.MarkedUpCost;
            subtotals.Flat += thisFeature.FlatCost;
            subtotals.Plus += thisFeature.UserMarkedUpCost;
            
            if (priceRequest.RefType == "Order")
            {
              AddFeaturePartsToOrder(order, thisFeature);

            }
            else if (priceRequest.RefType == "PriceReport")
            {
              // write out required part(s) to the report file
              sr.WriteLine($"{thisFeature.SKU},{thisFeature.Height},{thisFeature.Width},{thisFeature.ColorName},{thisFeature.ColorPerSquareFootCost},{thisFeature.LinearFootCost},{thisFeature.WholesalePrice},{thisFeature.Quantity},{thisFeature.WholesalePrice * thisFeature.Quantity},{thisFeature.ColorMarkup},{GlobalHelpers.Format(thisFeature.MarkedUpCost)}");
            }

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
          float width = thisPart.Width;
          float area = remainingWallHeight * width;
          LoadWallCost(defaultColor, ref thisTotalPartCost, ref thisPart, area);
          subtotals.Value += thisTotalPartCost;
          subtotals.Flat += thisPart.Cost;
          subtotals.Plus += thisTotalPartCost * (1 + thisUserMarkup / 100);

          if (priceRequest.RefType == "Order")
          {
            AddWallPartsToOrder(order, thisUserMarkup, thisTotalPartCost, thisPart);
          }
          else if (priceRequest.RefType == "PriceReport")
          {
            // write out required part(s) to the report file
            sr.WriteLine($"{thisPart.SKU},{remainingWallHeight},{width},{thisPart.ColorName},{thisPart.ColorPerSquareFootCost},{thisPart.LinearFootCost},{thisPart.Cost},{thisPart.Quantity},{thisPart.Cost * thisPart.Quantity},{thisPart.ColorMarkup},{GlobalHelpers.Format(thisTotalPartCost)}");
          }
        }
      }

      return new PriceGroup(subtotals.Value, subtotals.Flat, subtotals.Plus);
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

  private static void AddWallPartsToOrder(Order order, decimal thisUserMarkup, decimal thisTotalPartCost, Part thisPart)
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
      cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalPartCost - thisPart.Cost));
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalPartCost * (1 + thisUserMarkup / 100) - thisTotalPartCost));
      conn.Open();
      cmd.ExecuteNonQuery();
    }
  }

  private static void LoadWallCost(int defaultColor, ref decimal thisTotalPartCost, ref Part thisPart, float area)
  {
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

          thisPart.Cost = (decimal)area * thisPart.ColorPerSquareFootCost / 144;
          thisTotalPartCost = thisPart.Cost * (1 + thisPart.ColorMarkup / 100);
        }
      }
    }
  }

  private static void AddFeaturePartsToOrder(Order order, Feature thisFeature)
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
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisFeature.UserMarkedUpCost - thisFeature.MarkedUpCost));
      conn2.Open();
      cmd.ExecuteNonQuery();
    }
  }

  private static void LoadFeatureCostInfo(decimal thisUserMarkup, Feature thisFeature)
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
          thisFeature.ColorMarkup = dr.GetDecimal("PercentMarkup");
          thisFeature.ColorPerSquareFootCost = dr.GetDecimal("ColorPerSquareFoot");
          thisFeature.WholesalePrice = dr.GetDecimal("WholesalePrice");

          decimal areaInSf = thisFeature.AreaInSquareFeet;
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
  }

  private static DataTable LoadFeatureDataTable(int cabinetId)
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

  private static void AddOrderItem(Order order, decimal thisUserMarkup, decimal thisTotalPartCost, Part thisPart)
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
      cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalPartCost - thisPart.Cost));
      cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalPartCost * (1 + thisUserMarkup / 100) - thisTotalPartCost));
      conn.Open();
      cmd.ExecuteNonQuery();
    }
  }

  private static decimal GetCostForSku(string thisPartSku)
  {
    decimal thisPartCost = 0;
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM PricingSkus WHERE SKU = @sku";
      cmd.Parameters.AddWithValue("@sku", thisPartSku);
      conn.Open();
      using (SqliteDataReader dr = cmd.ExecuteReader())
      {
        if (dr.HasRows && dr.Read())
        {
          thisPartCost = dr.GetDecimal("WholesalePrice");
        }
      }
    }

    return thisPartCost;
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

  private static void CreateNewOrder(PriceRequest priceRequest, Order order)
  {
    // create a new order
    order.KitchenId = priceRequest.KitchenId;
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

  private static DataTable LoadWallsDataTable(PriceRequest priceRequest)
  {
    DataTable dt;
    using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
    {
      var cmd = conn.CreateCommand();
      cmd.CommandText = "SELECT * FROM Walls WHERE KitchenId = @kitchenId AND WallOrder = @wallOrderNum";
      cmd.Parameters.AddWithValue("@kitchenId", priceRequest.KitchenId);
      cmd.Parameters.AddWithValue("@wallOrderNum", priceRequest.WallOrderNum);
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
