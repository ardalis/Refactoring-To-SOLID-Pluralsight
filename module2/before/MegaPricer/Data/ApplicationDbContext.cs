using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MegaPricer.Data;

public class ApplicationDbContext : IdentityDbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<Cabinet> Cabinets { get; set; }
  public DbSet<Feature> Features { get; set; }
  public DbSet<Kitchen> Kitchens { get; set; }
  public DbSet<PricingColor> PricingColors { get; set; }
  public DbSet<UserMarkup> UserMarkups { get; set; }
  public DbSet<Wall> Walls { get; set; }
}
