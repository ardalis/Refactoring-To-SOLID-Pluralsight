using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MegaPricer.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Kitchen> Kitchens { get; set; }
    public DbSet<PricingColor> PricingColors { get; set; } 
}

public class Kitchen
{
    public int KitchenId { get; set; }
    public Guid UserId { get; set;}
    public string Name { get; set; }
    public List<Wall> Walls { get; set; } = new();
    public float BaseHeight { get; set; }
    public float BaseDepth { get; set; }

  internal static void GetCustomerKitchen(int kitchenId, string userName)
  {
  }
}

public class Wall
{
    public int WallId { get; set; }
    public int KitchenId { get; set; }
    public int WallOrder { get; set; }
    public string Name { get; set; }
    public int CabinetColor { get; set; }
    public int VertColor { get; set; }
    public int BackingColor { get; set; }

    public static Wall GetKitchenWallByOrderNum(int kitchenId, int wallOrderNum, string company)
    {
        return new Wall();
    }
}

public class PricingColor
{
    public int PricingColorId { get; set; }
    public float Price { get; set; }
    public float PercentMarkup { get; set; }
    public float ColorPerSquareFoot { get; set; }
    
    public float GetPrice(string mode)
    {
        if (mode == "Custom")
            return PercentMarkup;
        if (mode == "Standard")
            return ColorPerSquareFoot;
        return 0;
    }
}
