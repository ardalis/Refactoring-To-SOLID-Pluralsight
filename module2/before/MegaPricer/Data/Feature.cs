namespace MegaPricer.Data;

public class Feature
{
  public int FeatureId { get; set; }
  public int CabinetId { get; set; }
  public int FeatureOrder { get; set; }
  public string SKU { get; set; }
  public int Quantity { get; set; } = 1;
  public bool IsDoor { get; set; }
  public int Color { get; set; }
  public float Width { get; set; }
  public float Depth { get; set; }
  public float Height { get; set; }
}

