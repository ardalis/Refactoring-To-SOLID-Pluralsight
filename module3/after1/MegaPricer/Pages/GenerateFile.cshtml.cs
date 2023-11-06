﻿using MegaPricer.Data;
using MegaPricer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MegaPricer.Pages;

public class GenerateFileModel : PageModel
{
  private readonly ILogger<GenerateFileModel> _logger;

  public GenerateFileModel(ILogger<GenerateFileModel> logger)
  {
    _logger = logger;
  }

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

    string userName = User.Identity.Name;
    PriceRequest priceRequest = new()
    {
      kitchenId = 1,
      wallOrderNum = 1,
      userName = userName,
      refType = "PriceReport"
    };
    new PricingService().CalculatePrice(priceRequest);
  }
}
