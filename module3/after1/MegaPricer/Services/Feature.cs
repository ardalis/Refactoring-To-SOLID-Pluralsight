namespace MegaPricer.Services;

public record struct Feature
{
  public int FeatureId { get; set; }
  public int ColorId { get; set; }
  public string ColorName { get; set; }
  public decimal ColorMarkup { get; set; }
  public decimal ColorPerSquareFootCost { get; set; }
  public string SKU { get; set; }
  public int Quantity { get; set; }
  public float Width { get; set; }
  public float Height { get; set; }
  public decimal WholesalePrice { get; set; }
  public decimal FlatCost { get; set; }
  public decimal MarkedUpCost { get; set; }
  public decimal UserMarkedUpCost { get; set; }
  public decimal LinearFootCost { get; set; }
}
