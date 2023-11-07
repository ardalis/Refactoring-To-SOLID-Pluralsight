using System.Net.Mail;

namespace MegaPricer.Services;

public static class GlobalHelpers
{
  internal static void SendErrorEmail(string method, string errorMessage, string? stackTrace)
  {
    using (var client = new SmtpClient("localhost"))
    {
      var message = new MailMessage("donotreply@test.com", "errors@test.com");
      message.Subject = $"Error in {method}";
      message.Body = errorMessage + Environment.NewLine + stackTrace;
      client.Send(message);
    }
  }

  internal static decimal Format(float value)
  {
    return (decimal)Math.Round(value, 2);
  }
}
