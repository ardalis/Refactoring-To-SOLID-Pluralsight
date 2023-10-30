using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MegaPricer.Data;
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<Cabinet> Cabinets { get; set; }
  public DbSet<Feature> Features { get; set; }
  public DbSet<Kitchen> Kitchens { get; set; }
  public DbSet<Order> Orders { get; set; }
  public DbSet<PricingColor> PricingColors { get; set; }
  public DbSet<PricingSku> PricingSkus { get; set; }
  public DbSet<UserMarkup> UserMarkups { get; set; }
  public DbSet<Wall> Walls { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    SeedUserRoles(builder);
  }

  private void SeedUsers(ModelBuilder builder)
  {
    AppUser user = new()
    {
      Id = ConfigurationSettings.AdminUserId,
      UserName = "admin@test.com",
      NormalizedUserName = "admin@test.com".ToUpper(),
      Email = "admin@test.com",
      NormalizedEmail = "admin@test.com".ToUpper(),
      LockoutEnabled = false,
      CompanyShortName = "ACME",
      PricingOff = "N"
    };

    PasswordHasher<AppUser> passwordHasher = new();
    user.PasswordHash = passwordHasher.HashPassword(user, ConfigurationSettings.DefaultPassword);
    user.EmailConfirmed = true;

    builder.Entity<AppUser>().HasData(user);
  }

  private void SeedRoles(ModelBuilder builder)
  {
    builder.Entity<IdentityRole>().HasData(
        new IdentityRole() { Id = ConfigurationSettings.AdminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
        new IdentityRole() { Id = "a7b013f0-5201-4317-abd8-c211f91b7330", Name = "Sales", NormalizedName = "SALES" }
        );
  }

  private void SeedUserRoles(ModelBuilder builder)
  {
    SeedUsers(builder);
    SeedRoles(builder);

    builder.Entity<IdentityUserRole<string>>().HasData(
        new IdentityUserRole<string>()
        {
          RoleId = ConfigurationSettings.AdminRoleId,
          UserId = ConfigurationSettings.AdminUserId
        }
        );
  }
}
