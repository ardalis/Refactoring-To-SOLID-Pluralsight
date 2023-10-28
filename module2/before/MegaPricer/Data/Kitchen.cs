namespace MegaPricer.Data;

public class Kitchen
{
    public int KitchenId { get; set; }
    public Guid UserId { get; set;}
    public string Name { get; set; }
    public List<Wall> Walls { get; set; } = new();
    public float BaseHeight { get; set; }
    public float BaseDepth { get; set; }

  internal static void GetCustomerKitchen(int kitchenId, string userName)
  {
  }
}
