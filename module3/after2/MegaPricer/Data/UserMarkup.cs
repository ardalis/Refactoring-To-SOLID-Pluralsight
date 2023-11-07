namespace MegaPricer.Data;

public class UserMarkup
{
  public int UserMarkupId { get; set; }
  public string UserName { get; set; }
  public int MarkupPercent { get; set; }
  public bool UseCustomPricing { get; set; }
}

