namespace MegaPricer.Models;

public class Wall
{
  public int DefaultColor { get; set; } = 0;
  public bool IsIsland { get; set; } = false;
  public int WallId { get; set; } = 0;
  public float WallHeight { get; set; } = 0;
}
