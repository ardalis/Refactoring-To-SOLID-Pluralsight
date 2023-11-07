namespace MegaPricer.Data;

public class Cabinet
{
  public int CabinetId { get; set; }
  public int WallId { get; set; }
  public int CabinetOrder { get; set; }
  public int Color { get; set; }
  public string SKU { get; set; }
  public float TopOffset { get; set; }
  public float LeftOffset { get; set; }
  public float Width { get; set; }
  public float Depth { get; set; }
  public float Height { get; set; }

  public List<Feature> Features { get; set; } = new();
}

