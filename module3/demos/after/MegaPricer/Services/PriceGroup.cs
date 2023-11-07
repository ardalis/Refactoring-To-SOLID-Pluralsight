namespace MegaPricer.Services;

public record PriceGroup(decimal Subtotal, decimal SubtotalFlat, decimal SubtotalPlus)
{
  public override string ToString()
  {
    return String.Format("{0:C2}|{1:C2}|{2:C2}", Subtotal, SubtotalFlat, SubtotalPlus);
  }
}
