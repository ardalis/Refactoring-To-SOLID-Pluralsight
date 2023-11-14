using System.ComponentModel;
using MegaPricer.Services;

namespace MegaPricer.IntegrationTests;

public class PriceReportCalculationStrategy_AddFeature
{
  [Fact]
  public void AddsFeatureLineToReport()
  {
    string testSku = Guid.NewGuid().ToString();
    var testFeature = new Models.Feature()
    {
      SKU = testSku
    };
    using (var service = new PriceReportCalculationStrategy())
    {
      service.Create(new Data.Kitchen());
      service.AddFeature(testFeature, 0);
    }

    string baseDirectory = AppContext.BaseDirectory;
    string path = baseDirectory + "Orders.csv";

    var contents = File.ReadAllText(path);
    Assert.Contains(testSku, contents);
  }
}
