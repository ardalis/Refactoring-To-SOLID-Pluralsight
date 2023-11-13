namespace MegaPricer.Models;

public record PriceGroup(decimal Subtotal, decimal SubtotalFlat, decimal SubtotalPlus)
{
  public override string ToString()
  {
    return string.Format("{0:C2}|{1:C2}|{2:C2}", Subtotal, SubtotalFlat, SubtotalPlus);
  }
}
