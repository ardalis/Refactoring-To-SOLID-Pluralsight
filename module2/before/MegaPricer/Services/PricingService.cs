using System.Data;
using System.IO;
using MegaPricer.Data;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;

namespace MegaPricer.Services;

public class PricingService
{
  public static string CalculatePrice(int kitchenId, int wallOrderNum, string userName, string refType)
  {
    if (Context.Session[userName]["PricingOff"] == "Y") return "0|0|0";

    Kitchen kitchen = new Kitchen();
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
    DataTable dt = new DataTable();
    DataTable dt2 = new DataTable();
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
      Kitchen.GetCustomerKitchen(kitchenId, userName);
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
        string path = baseDirectory + DateTime.Today.ToString("yyyy-MM-dd") + "_Orders.csv";
        sr = new StreamWriter(path);
        sr.WriteLine($"{kitchen.Name} ({kitchen.KitchenId}) - Run time: {DateTime.Now.ToLongTimeString()} ");
        sr.WriteLine("");
        sr.WriteLine("Part Name,Part SKU,Height,Width,Depth,Color,Sq Ft $, Lin Ft $,Per Piece $,# Needed,Part Price,Add On %,Total Part Price");
      }

      defaultColor = Convert.ToInt32(dt.Rows[0]["CabinetColor"]);// dt.Rows[0].Field<int>("CabinetColor");
      wallId = Convert.ToInt32(dt.Rows[0]["WallId"]);
      isIsland = Convert.ToBoolean(dt.Rows[0]["IsIsland"]);
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

      foreach (DataRow row in dt2.Rows) // each cabinet
      {
        thisPartWidth = Convert.ToSingle(row["Width"]); // row.Field<float>("Width");
        thisPartDepth = Convert.ToSingle(row["Depth"]); // row.Field<float>("Depth");
        thisPartHeight = Convert.ToSingle(row["Height"]); // row.Field<float>("Height");
        thisPartColor = Convert.ToInt32(row["Color"]); // row.Field<int>("Color");
        thisPartSku = row.Field<string>("SKU");
        thisPartCost = 0;
        thisSectionWidth = 0;

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
      }
      if (!isIsland)
      {
        // price wall color backing around cabinets

      }

      if (refType == "Order")
      {
        //var Order = new();
        //Order.SaveOrder(kitchenId, wallOrderNum, userName); 
      }
      else if (refType == "PriceReport")
      {
        // write out required part(s) to the report file
        sr.WriteLine($"{thisPartSku},{thisPartHeight},{thisPartWidth},{thisPartDepth},{thisPartColorName},{thisColorSquareFoot},{thisLinearFootCost},{thisPartCost},{thisPartQty},{thisPartCost*thisPartQty},{thisColorMarkup},{GlobalHelpers.Format(thisTotalPartCost)}");
      }
      else
      {
        // Just get the cost
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
