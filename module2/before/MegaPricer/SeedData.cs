using MegaPricer.Data;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
  public static void Initialize(IServiceProvider serviceProvider)
  {
    using (var dbContext = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
    {
      // Look for any Kitchens.
      if (dbContext.Kitchens.Any())
      {
        return;   // DB has been seeded
      }

      PopulateTestData(dbContext);
    }
  }

  public static Kitchen GetKitchen1()
  {
    var kitchen = new Kitchen();
    kitchen.Name = "Smiths Kitchen";
    kitchen.BaseHeight = 0.25f;
    kitchen.BaseDepth = 0.25f;
    kitchen.UserId = Guid.Parse(ConfigurationSettings.AdminUserId);

    int order = 1;
    kitchen.Walls.Add(new Wall() { Name = "North wall", WallOrder = order++, BackingColor=1 });
    kitchen.Walls.Add(new Wall() { Name = "Northeast wall", WallOrder = order++, BackingColor = 1 });
    kitchen.Walls.Add(new Wall() { Name = "East wall", WallOrder = order++, BackingColor = 1 });
    kitchen.Walls.Add(new Wall() { Name = "South wall", WallOrder = order++, BackingColor = 1 });
    kitchen.Walls.Add(new Wall() { Name = "Island", WallOrder = order++, IsIsland = true, BackingColor = 0 });

    return kitchen;
  }

  public static void PopulateTestData(ApplicationDbContext dbContext)
  {
    PopulatePricingColors(dbContext);

    foreach (var item in dbContext.Kitchens)
    {
      dbContext.Remove(item);
    }
    dbContext.SaveChanges();

    var kitchen1 = GetKitchen1();
    dbContext.Kitchens.Add(kitchen1);

    dbContext.SaveChanges();

    var cabinet1 = new Cabinet() 
    { 
      Width = 36.0f,
      Depth = 12.0f,
      Height = 24.0f,
      CabinetOrder= 1,
      Color = 1,
      TopOffset = 0.0f,
      LeftOffset = 0.0f,
      SKU = "CAB-1"
    };
    kitchen1.Walls[0].Cabinets.Add(cabinet1);

    dbContext.SaveChanges();

    var feature1 = new Feature()
    {
      CabinetId = cabinet1.CabinetId,
      FeatureOrder = 1,
      Color = 2,
      Depth = 11.5f,
      Height = 0.25f,
      Width = 23.0f,
      IsDoor = false,
      SKU = "GLSHLF-1"
    };
    cabinet1.Features.Add(feature1);

    var feature2 = new Feature()
    {
      CabinetId = cabinet1.CabinetId,
      FeatureOrder = 2,
      Color = 1,
      Depth = 0.5f,
      Height = 23.5f,
      Width = 17.5f,
      IsDoor = true,
      SKU = "DR-1"
    }; // left door
    cabinet1.Features.Add(feature2);
    var feature3 = new Feature()
    {
      CabinetId = cabinet1.CabinetId,
      FeatureOrder = 3,
      Color = 1,
      Depth = 0.5f,
      Height = 23.5f,
      Width = 17.5f,
      IsDoor = true,
      SKU = "DR-1"
    }; // right door
    cabinet1.Features.Add(feature3);
    var feature4 = new Feature()
    {
      CabinetId = cabinet1.CabinetId,
      FeatureOrder = 4,
      Color = 3,
      Quantity = 2,
      SKU = "BR-KNB"
    }; // door hardware
    cabinet1.Features.Add(feature4);

    dbContext.SaveChanges();
  }

  public static void PopulatePricingColors(ApplicationDbContext dbContext)
  {
    var pricingColor1 = new PricingColor()
    { 
       Name = "White",
       ColorPerSquareFoot = 0.10f,
       PercentMarkup= 5,
       WholesalePrice = 0.0f
    };
    dbContext.PricingColors.Add(pricingColor1);
    dbContext.SaveChanges();

    var pricingColor2 = new PricingColor()
    {
      Name = "Glass",
      ColorPerSquareFoot = 9.9f,
      PercentMarkup = 10,
      WholesalePrice = 0.0f
    };
    dbContext.PricingColors.Add(pricingColor2);
    dbContext.SaveChanges();

    var pricingColor3 = new PricingColor()
    {
      Name = "Brass",
      ColorPerSquareFoot = 0.0f,
      PercentMarkup = 10,
      WholesalePrice = 2.0f
    };
    dbContext.PricingColors.Add(pricingColor3);
    dbContext.SaveChanges();
  }
}
