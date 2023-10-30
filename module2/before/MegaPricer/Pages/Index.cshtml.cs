using MegaPricer.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MegaPricer.Pages;

public class IndexModel : PageModel
{
  private readonly ILogger<IndexModel> _logger;
  private readonly ApplicationDbContext _dbContext;

  public IndexModel(ILogger<IndexModel> logger,
        ApplicationDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  public Kitchen Kitchen { get; set; } = new();
  public string Price { get; set; }

  public void OnGet()
  {
    if (!(User is null) && User.Identity.IsAuthenticated)
    {
      if (!Context.Session.ContainsKey(User.Identity.Name))
      {
        Context.Session.Add(User.Identity.Name, new Dictionary<string, object>());
      }
      if (!Context.Session[User.Identity.Name].ContainsKey("CompanyShortName"))
      {
        Context.Session[User.Identity.Name].Add("CompanyShortName", "Acme");
      }
      if (!Context.Session[User.Identity.Name].ContainsKey("PricingOff"))
      {
        Context.Session[User.Identity.Name].Add("PricingOff", "N");
      }
    }

    var kitchen = _dbContext.Kitchens
        .Include(k => k.Walls)
        .ThenInclude(w => w.Cabinets)
        .ThenInclude(c => c.Features)
        .First();
    if (kitchen != null) { Kitchen = kitchen; }

    //Price = PricingService.CalculatePrice(kitchen.KitchenId, 1, User.Identity.Name, "");
  }
}
