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

        kitchen.Walls.Add(new Wall() { Name = "North wall" });
        kitchen.Walls.Add(new Wall() { Name = "Northeast wall" });
        kitchen.Walls.Add(new Wall() { Name = "East wall" });
        kitchen.Walls.Add(new Wall() { Name = "South wall" });

        return kitchen;
    }

    public static void PopulateTestData(ApplicationDbContext dbContext)
    {
        foreach (var item in dbContext.Kitchens)
        {
            dbContext.Remove(item);
        }
        dbContext.SaveChanges();

        dbContext.Kitchens.Add(GetKitchen1());

        dbContext.SaveChanges();
    }
}
