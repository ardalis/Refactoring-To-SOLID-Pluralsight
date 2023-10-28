namespace MegaPricer.Data;

public class PricingColor
{
  public int PricingColorId { get; set; }
  public string SKU { get; set; }
  public float Price { get; set; }
  public float PercentMarkup { get; set; }
  public float ColorPerSquareFoot { get; set; }

  public float GetPrice(string mode)
  {
    if (mode == "Custom")
      return PercentMarkup;
    if (mode == "Standard")
      return ColorPerSquareFoot;
    return 0;
  }
}
