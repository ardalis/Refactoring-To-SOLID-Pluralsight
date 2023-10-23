using System.Data;
using System.Net.Mail;
using MegaPricer.Data;

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
        float bbHeight = 0;
        float bbDepth = 0;
        DataTable dt = new DataTable();

        Context.Session[userName]["WallWeight"] = 0;

        try
        {
            if(wallOrderNum == 0)
            {
                return "Session expired: Log in again.";
            }
            if(kitchenId <= 0)
            {
                return "invalid kitchenId";
            }
            Kitchen.GetCustomerKitchen(kitchenId, userName);
            bbHeight = kitchen.BaseHeight;
            bbDepth = kitchen.BaseDepth;

            return String.Format("{0:C2}|{1:C2}|{2:C2}", subtotal, subtotalFlat, subtotalPlus);
        }
        catch (Exception ex)
        {
            GlobalHelpers.SendErrorEmail("CalcPrice", ex.Message, ex.StackTrace);
        }

        return "";
    }
}

public static class GlobalHelpers
{
  internal static void SendErrorEmail(string method, string errorMessage, string? stackTrace)
  {
        using(var client = new SmtpClient("localhost"))
        {
            var message = new MailMessage("donotreply@test.com", "errors@test.com");
            message.Subject = $"Error in {method}";
            message.Body = errorMessage + Environment.NewLine + stackTrace;
            client.Send(message);
        }
  }
}
