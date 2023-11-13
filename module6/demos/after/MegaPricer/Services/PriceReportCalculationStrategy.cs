using MegaPricer.Data;
using MegaPricer.Interfaces;
using MegaPricer.Models;
using Feature = MegaPricer.Models.Feature;

namespace MegaPricer.Services;

public class PriceReportCalculationStrategy : IPriceCalculationStrategy, IDisposable
{
  private TextWriter _streamWriter;


  public void AddFeature(Feature feature, decimal userMarkup)
  {
    _streamWriter.WriteLine($"{feature.SKU},{feature.Height},{feature.Width},{feature.ColorName},{feature.ColorPerSquareFootCost},{feature.LinearFootCost},{feature.WholesalePrice},{feature.Quantity},{feature.WholesalePrice * feature.Quantity},{feature.ColorMarkup},{GlobalHelpers.Format(feature.MarkedUpCost)}");
  }

  public void AddPart(Part part, decimal userMarkup)
  {
    _streamWriter.WriteLine($"{part.SKU},{part.Height},{part.Width},{part.Depth},{part.ColorName},{part.ColorPerSquareFootCost},{part.LinearFootCost},{part.Cost},{part.Quantity},{part.Cost * part.Quantity},{part.ColorMarkup},{GlobalHelpers.Format(part.MarkedUpCost)}");
  }

  public void AddWallTreatment(Part part, decimal userMarkup, float remainingWallHeight, float width)
  {
    _streamWriter.WriteLine($"{part.SKU},{remainingWallHeight},{width},{part.ColorName},{part.ColorPerSquareFootCost} , {part.LinearFootCost},{part.Cost},{part.Quantity},{part.Cost * part.Quantity},{part.ColorMarkup},{GlobalHelpers.Format(part.MarkedUpCost)}");
  }

  public void Create(Kitchen kitchen)
  {
    ConfigureStreamWriter();
    _streamWriter.WriteLine($"{kitchen.Name} ({kitchen.KitchenId}) - Run time: {DateTime.Now.ToLongTimeString()} ");
    _streamWriter.WriteLine("");
    _streamWriter.WriteLine("Part Name,Part SKU,Height,Width,Depth,Color,Sq Ft $, Lin Ft $,Per Piece $,# Needed,Part Price,Add On %,Total Part Price");
  }

  private void ConfigureStreamWriter()
  {
    if (_streamWriter != null) return;

    // Start writing to the report file
    string baseDirectory = AppContext.BaseDirectory;
    string path = baseDirectory + "Orders.csv";
    _streamWriter = new StreamWriter(path);
  }

  public void ReplaceDefaultStreamWriter(TextWriter streamWriter)
  {
    _streamWriter = streamWriter;
  }

  public void Dispose()
  {
    if (_streamWriter != null)
    {
      _streamWriter.Close();
      _streamWriter.Dispose();
    }
  }
}
