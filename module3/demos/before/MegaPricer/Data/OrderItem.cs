namespace MegaPricer.Data;

public class OrderItem
{
  public int OrderItemId { get; set; }
  public int OrderId { get; set; }
  public string SKU { get; set; }
  public int Quantity { get; set; }
  public float BasePrice { get; set; }
  public float Markup { get; set; }
  public float UserMarkup { get; set; }
}
