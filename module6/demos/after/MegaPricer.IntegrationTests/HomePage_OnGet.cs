using System.Xml;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MegaPricer.IntegrationTests;

public class HomePage_OnGet
{
  [Fact]
  public async Task IncludesExpectedPrices()
  {
    var factory = new MegaPricerWebApplicationFactory();
    var client = factory.CreateClient(new WebApplicationFactoryClientOptions() 
    { AllowAutoRedirect = false,HandleCookies=true});

    // get login page and extract antiforgery token
    var loginGetResponse = await client.GetAsync("/Identity/Account/Login");
    var loginGetContent = await loginGetResponse.Content.ReadAsStringAsync();

    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(loginGetContent);

    var requestVerificationToken = htmlDocument.DocumentNode
        .SelectSingleNode("//input[@name='__RequestVerificationToken']")
        .GetAttributeValue("value", "");

    var loginData = new Dictionary<string, string>
    {
      ["Input.Email"] = "admin@test.com",
      ["Input.Password"] = "Pass@word1",
      ["__RequestVerificationToken"] = requestVerificationToken
    };
    var loginContent = new FormUrlEncodedContent(loginData);
    //client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

    var loginPostResponse = await client.PostAsync("/Identity/Account/Login", loginContent);

    var response = await client.GetAsync("/");
    var content = await response.Content.ReadAsStringAsync();

    Assert.Contains("$123.56", content);  // marked up
    Assert.Contains("|$117.66|", content);// flat
    Assert.Contains("$135.92", content);  // plus
  }
}
