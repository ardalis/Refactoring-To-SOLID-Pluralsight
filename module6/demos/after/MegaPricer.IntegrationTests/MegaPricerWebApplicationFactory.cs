using System;
using MegaPricer.Data;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MegaPricer.IntegrationTests;

public class MegaPricerWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Development");

    return base.CreateHost(builder);
  }

  private static void SeedDatabase(IHost host)
  {
    using (var scope = host.Services.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<ApplicationDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<MegaPricerWebApplicationFactory>>();

      db.Database.EnsureCreated();

      try
      {
        SeedData.Initialize(scopedServices);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }
  }
}
