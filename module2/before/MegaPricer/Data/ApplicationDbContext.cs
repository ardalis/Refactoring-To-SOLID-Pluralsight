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
}

public class Kitchen
{
    public int KitchenId { get; set; }
    public Guid UserId { get; set;}
    public string Name { get; set; }
    public List<Wall> Walls { get; set; } = new();
}

public class Wall
{
    public int WallId { get; set; }
    public int KitchenId { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; }
    public int CabinetColor { get; set; }
    public int VertColor { get; set; }
    public int BackingColor { get; set; }

    public static Wall GetKitchenWallByOrderNum(int kitchenId, int wallOrderNum, string company)
    {
        return new Wall();
    }
}
