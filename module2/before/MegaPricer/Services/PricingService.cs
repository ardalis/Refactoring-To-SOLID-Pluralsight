using System.Data;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;

namespace MegaPricer.Services;

public class PricingService
{
  public static string CalculatePrice(int kitchenId, int wallOrderNum, string userName, string refType)
  {
    if (Context.Session[userName]["PricingOff"] == "Y") return "0|0|0";

    Kitchen kitchen = new Kitchen();
    Order order = new Order();
    float subtotal = 0;
    float subtotalFlat = 0;
    float subtotalPlus = 0;
    float grandtotal = 0;
    float grandtotalFlat = 0;
    float thisPartWidth = 0;
    float thisPartDepth = 0;
    float thisPartHeight = 0;
    float thisPartCost = 0;
    float thisSectionWidth = 0;
    string thisPartSku = "";
    float bbHeight = 0;
    float bbDepth = 0;
    int defaultColor = 0;
    int thisPartColor = 0;
    string thisPartColorName = "";
    float thisColorMarkup = 0;
    float thisColorSquareFoot = 0;
    float thisLinearFootCost = 0;
    float thisUserMarkup = 0;
    int thisPartQty = 0;
    float thisTotalPartCost = 0;
    bool isIsland = false;
    int wallId = 0;
    float wallHeight = 0;
    DataTable dt = new DataTable();
    DataTable dt2 = new DataTable();
    DataTable dt3 = new DataTable();
    StreamWriter sr = null;

    Context.Session[userName]["WallWeight"] = 0;

    try
    {
      if (wallOrderNum == 0)
      {
        return "Session expired: Log in again.";
      }
      if (kitchenId <= 0)
      {
        return "invalid kitchenId";
      }
      kitchen.GetCustomerKitchen(kitchenId, userName);
      bbHeight = kitchen.BaseHeight;
      bbDepth = kitchen.BaseDepth;
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

      if (dt.Rows.Count == 0)
      {
        return "invalid wallOrderNum";
      }

      if (refType == "PriceReport")
      {
        // Start writing to the report file
        string baseDirectory = AppContext.BaseDirectory;
        string path = baseDirectory + "Orders.csv";
        sr = new StreamWriter(path);
        sr.WriteLine($"{kitchen.Name} ({kitchen.KitchenId}) - Run time: {DateTime.Now.ToLongTimeString()} ");
        sr.WriteLine("");
        sr.WriteLine("Part Name,Part SKU,Height,Width,Depth,Color,Sq Ft $, Lin Ft $,Per Piece $,# Needed,Part Price,Add On %,Total Part Price");
      }
      else if (refType == "Order")
      {
        // create a new order
        order.KitchenId = kitchenId;
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

      defaultColor = Convert.ToInt32(dt.Rows[0]["CabinetColor"]);// dt.Rows[0].Field<int>("CabinetColor");
      wallId = Convert.ToInt32(dt.Rows[0]["WallId"]);
      isIsland = Convert.ToBoolean(dt.Rows[0]["IsIsland"]);
      wallHeight = Convert.ToSingle(dt.Rows[0]["Height"]);

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

      float totalCabinetHeight = 0;
      foreach (DataRow row in dt2.Rows) // each cabinet
      {
        int cabinetId = Convert.ToInt32(row["CabinetId"]);
        thisPartWidth = Convert.ToSingle(row["Width"]); // row.Field<float>("Width");
        thisPartDepth = Convert.ToSingle(row["Depth"]); // row.Field<float>("Depth");
        thisPartHeight = Convert.ToSingle(row["Height"]); // row.Field<float>("Height");
        thisPartColor = Convert.ToInt32(row["Color"]); // row.Field<int>("Color");
        thisPartSku = row.Field<string>("SKU");
        thisPartCost = 0;
        thisSectionWidth = 0;
        totalCabinetHeight += thisPartHeight;

        if (!String.IsNullOrEmpty(thisPartSku))
        {
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
                thisPartCost = dr.GetFloat("WholesalePrice");
              }
            }
          }
          using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
          {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM PricingColors WHERE PricingColorId = @pricingColorId";
            cmd.Parameters.AddWithValue("@pricingColorId", thisPartColor);
            conn.Open();
            using (SqliteDataReader dr = cmd.ExecuteReader())
            {
              if (dr.HasRows && dr.Read())
              {
                thisPartColorName = dr.GetString("Name");
                thisColorMarkup = dr.GetFloat("PercentMarkup");
                thisColorSquareFoot = dr.GetFloat("ColorPerSquareFoot");
              }
            }
          }
          thisTotalPartCost = thisPartCost * (1 + thisColorMarkup / 100);
          subtotal += thisTotalPartCost;
          subtotalFlat += thisPartCost;

          using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
          {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM UserMarkups WHERE UserName = @userName";
            cmd.Parameters.AddWithValue("@userName", userName);
            conn.Open();
            using (SqliteDataReader dr = cmd.ExecuteReader())
            {
              if (dr.HasRows && dr.Read())
              {
                thisUserMarkup = dr.GetFloat("MarkupPercent");
              }
            }
          }
          subtotalPlus = thisTotalPartCost * (1 + thisUserMarkup / 100);
        }

        if (refType == "Order")
        {
          // add this part to the order
          using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
          {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
            cmd.Parameters.AddWithValue("@orderId", order.OrderId);
            cmd.Parameters.AddWithValue("@sku", thisPartSku);
            cmd.Parameters.AddWithValue("@quantity", thisPartQty == 0 ? 1 : thisPartQty);
            cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(thisPartCost));
            cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalPartCost - thisPartCost));
            cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalPartCost * (1 + thisUserMarkup / 100) - thisTotalPartCost));
            conn.Open();
            cmd.ExecuteNonQuery();
          }
        }
        else if (refType == "PriceReport")
        {
          // write out required part(s) to the report file
          sr.WriteLine($"{thisPartSku},{thisPartHeight},{thisPartWidth},{thisPartDepth},{thisPartColorName},{thisColorSquareFoot},{thisLinearFootCost},{thisPartCost},{thisPartQty},{thisPartCost * thisPartQty},{thisColorMarkup},{GlobalHelpers.Format(thisTotalPartCost)}");
        }
        else
        {
          // Just get the cost
        }

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

          conn.Close();
          foreach (DataRow featureRow in dt3.Rows)
          {
            int featureId = Convert.ToInt32(featureRow["FeatureId"]);
            int colorId = Convert.ToInt32(featureRow["Color"]);
            string featureSKU = Convert.ToString(featureRow["SKU"]);
            int quantity = Convert.ToInt32(featureRow["Quantity"]);
            float featureHeight = Convert.ToSingle(featureRow["Height"]);
            float featureWidth = Convert.ToSingle(featureRow["Width"]);
            float featureCost = 0;
            float thisTotalFeatureCost = 0;
            string featureColorName = "";
            float wholesalePrice = 0;

            if (colorId > 0)
            {
              cmd = conn.CreateCommand();
              cmd.CommandText = "SELECT * FROM PricingColors WHERE PricingColorId = @pricingColorId";
              cmd.Parameters.AddWithValue("@pricingColorId", colorId);
              conn.Open();
              using (SqliteDataReader dr = cmd.ExecuteReader())
              {
                if (dr.HasRows && dr.Read())
                {
                  featureColorName = dr.GetString("Name");
                  float colorMarkup = dr.GetFloat("PercentMarkup");
                  thisColorSquareFoot = dr.GetFloat("ColorPerSquareFoot");
                  wholesalePrice = dr.GetFloat("WholesalePrice");

                  float areaInSf = featureHeight * featureWidth / 144;
                  featureCost = areaInSf * thisColorSquareFoot;
                  if (featureCost == 0)
                  {
                    featureCost = quantity * wholesalePrice;
                  }
                  thisTotalFeatureCost = featureCost * (1 + thisColorMarkup / 100);
                  subtotal += thisTotalFeatureCost;
                  subtotalFlat += featureCost;
                  subtotalPlus = thisTotalFeatureCost * (1 + thisUserMarkup / 100);
                }
              }
              if (refType == "Order")
              {
                // add this part to the order
                using (var conn2 = new SqliteConnection(ConfigurationSettings.ConnectionString))
                {
                  cmd = conn2.CreateCommand();
                  cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
                  cmd.Parameters.AddWithValue("@orderId", order.OrderId);
                  cmd.Parameters.AddWithValue("@sku", featureSKU);
                  cmd.Parameters.AddWithValue("@quantity", quantity == 0 ? 1 : quantity);
                  cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(featureCost));
                  cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalFeatureCost - featureCost));
                  cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalFeatureCost * (1 + thisUserMarkup / 100) - thisTotalFeatureCost));
                  conn2.Open();
                  cmd.ExecuteNonQuery();
                }

              }
              else if (refType == "PriceReport")
              {
                // write out required part(s) to the report file
                sr.WriteLine($"{featureSKU},{featureHeight},{featureWidth},{featureColorName},{thisColorSquareFoot},{thisLinearFootCost},{wholesalePrice},{quantity},{wholesalePrice * quantity},{thisColorMarkup},{GlobalHelpers.Format(thisTotalFeatureCost)}");
              }

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
          float width = thisPartWidth;
          float area = remainingWallHeight * width;
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
                thisPartSku = "PAINT";
                thisPartColorName = dr.GetString("Name");
                thisColorMarkup = dr.GetFloat("PercentMarkup");
                thisColorSquareFoot = dr.GetFloat("ColorPerSquareFoot");

                thisPartCost = area * thisColorSquareFoot / 144;
                thisTotalPartCost = thisPartCost * (1 + thisColorMarkup / 100);
                subtotal += thisTotalPartCost;
                subtotalFlat += thisPartCost;
                subtotalPlus = thisTotalPartCost * (1 + thisUserMarkup / 100);
              }
            }
          }
          if (refType == "Order")
          {
            // add this part to the order
            using (var conn = new SqliteConnection(ConfigurationSettings.ConnectionString))
            {
              var cmd = conn.CreateCommand();
              cmd.CommandText = "INSERT INTO ORDERITEM (OrderId,SKU,Quantity,BasePrice,Markup,UserMarkup) VALUES (@orderId,@sku,@quantity,@basePrice,@markup,@userMarkup)";
              cmd.Parameters.AddWithValue("@orderId", order.OrderId);
              cmd.Parameters.AddWithValue("@sku", thisPartSku);
              cmd.Parameters.AddWithValue("@quantity", thisPartQty == 0 ? 1 : thisPartQty);
              cmd.Parameters.AddWithValue("@basePrice", GlobalHelpers.Format(thisPartCost));
              cmd.Parameters.AddWithValue("@markup", GlobalHelpers.Format(thisTotalPartCost - thisPartCost));
              cmd.Parameters.AddWithValue("@userMarkup", GlobalHelpers.Format(thisTotalPartCost * (1 + thisUserMarkup / 100) - thisTotalPartCost));
              conn.Open();
              cmd.ExecuteNonQuery();
            }

          }
          else if (refType == "PriceReport")
          {
            // write out required part(s) to the report file
            sr.WriteLine($"{thisPartSku},{remainingWallHeight},{width},{thisPartColorName},{thisColorSquareFoot},{thisLinearFootCost},{thisPartCost},{thisPartQty},{thisPartCost * thisPartQty},{thisColorMarkup},{GlobalHelpers.Format(thisTotalPartCost)}");
          }
        }
      }


      return String.Format("{0:C2}|{1:C2}|{2:C2}", subtotal, subtotalFlat, subtotalPlus);
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
}
