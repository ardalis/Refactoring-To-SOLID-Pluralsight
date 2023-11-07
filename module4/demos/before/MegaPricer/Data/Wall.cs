namespace MegaPricer.Data;

public class Wall
{
  public int WallId { get; set; }
  public int KitchenId { get; set; }
  public int WallOrder { get; set; }
  public string Name { get; set; }
  public int CabinetColor { get; set; }
  public int VertColor { get; set; }
  public int BackingColor { get; set; }
  public List<Cabinet> Cabinets { get; set; } = new();
  public bool IsIsland { get; set; }
  public float Height { get; set; }
}

