namespace MegaPricer.Data;

public class Order
{
  public int OrderId { get; set; }
  public int KitchenId { get; set; }
  public DateTime OrderDate { get; set; } = DateTime.Today;
  public string OrderStatus { get; set; } = "New";
  public string OrderType { get; set; } = "Kitchen";
  public List<OrderItem> orderItems { get; set; } = new();
}
