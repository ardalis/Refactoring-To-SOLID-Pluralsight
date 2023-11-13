using Microsoft.AspNetCore.Identity;

namespace MegaPricer.Data;

public class AppUser : IdentityUser
{
  public string CompanyShortName { get; set; }
  public string PricingOff { get; set; }
}
