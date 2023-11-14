using System.ComponentModel;
using MegaPricer.Data;
using MegaPricer.Services;

namespace MegaPricer.IntegrationTests;

public class PriceReportCalculationStrategy_Create
{
  [Fact]
  [Category("Integration")]
  public void CreatesCSVFile()
  {
    using (var service = new PriceReportCalculationStrategy())
    {
      service.Create(new Kitchen());
    }

    string baseDirectory = AppContext.BaseDirectory;
    string path = baseDirectory + "Orders.csv";

    Assert.True(File.Exists(path));
  }
}
