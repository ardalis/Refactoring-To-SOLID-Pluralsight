﻿using MegaPricer.Data;
using MegaPricer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MegaPricer.Pages;

public class PlaceOrderModel : PageModel
{
  private readonly ILogger<PlaceOrderModel> _logger;

  public PlaceOrderModel(ILogger<PlaceOrderModel> logger)
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
    var priceRequest = new PriceRequest
    {
      KitchenId = 1,
      WallOrderNum = 1,
      UserName = userName,
      RefType = "Order"
    };
    new PricingService().CalculatePrice(priceRequest);
  }
}
