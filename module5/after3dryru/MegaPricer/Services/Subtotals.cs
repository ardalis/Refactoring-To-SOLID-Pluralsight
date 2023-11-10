namespace MegaPricer.Services;

public record struct Subtotals()
{
  public decimal Value { get; set; }
  public decimal Flat { get; set; }
  public decimal Plus { get; set; }
}
