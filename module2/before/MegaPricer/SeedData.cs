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

    int order = 1;
    kitchen.Walls.Add(new Wall() { Name = "North wall", WallOrder = order++ });
    kitchen.Walls.Add(new Wall() { Name = "Northeast wall", WallOrder = order++ });
    kitchen.Walls.Add(new Wall() { Name = "East wall", WallOrder = order++ });
    kitchen.Walls.Add(new Wall() { Name = "South wall", WallOrder = order++ });
    kitchen.Walls.Add(new Wall() { Name = "Island", WallOrder = order++, IsIsland = true });

    return kitchen;
  }

  public static void PopulateTestData(ApplicationDbContext dbContext)
  {
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
      LeftOffset = 0.0f
    };
    kitchen1.Walls[0].Cabinets.Add(cabinet1);


  }
}
