namespace MegaPricer.Models;

public class Part
{
  public int CabinetId { get; set; }
  public float Width { get; set; }
  public float Height { get; set; }
  public float Depth { get; set; }
  public int ColorId { get; set; }
  public string ColorName { get; set; }
  public decimal ColorMarkup { get; set; }
  public decimal ColorPerSquareFootCost { get; set; }
  public decimal LinearFootCost { get; set; }
  public string SKU { get; set; }
  public int Quantity { get; set; }
  public decimal Cost { get; set; }
  public decimal MarkedUpCost { get; set; }

  public void ApplyMarkup(decimal markup)
  {
    MarkedUpCost = Cost * (1 + markup / 100);
  }
}
