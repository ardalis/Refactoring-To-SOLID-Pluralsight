﻿using MegaPricer.Data;
using MegaPricer.Interfaces;
using MegaPricer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages(options =>
{
  options.Conventions.AuthorizeFolder("/");
});

// App Services
builder.Services.AddScoped<IOrderDataService, SqliteOrderDataService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<IPricingService, PricingServiceDecorator>();
builder.Services.AddScoped<IPriceCalculationStrategy, DefaultPriceCalculationStrategy>();
builder.Services.AddScoped<PriceReportCalculationStrategy>();
builder.Services.AddScoped<NewOrderPriceCalculationStrategy>();
builder.Services.AddScoped<IGetUserMarkup, SqliteGetUserMarkupService>();
builder.Services.AddScoped<IKitchenDataService, SqliteKitchenDataService>();
builder.Services.AddScoped<IWallDataService, SqliteWallDataService>();
builder.Services.AddScoped<ICabinetDataService, SqliteCabinetDataService>();
builder.Services.AddScoped<IPartCostDataService, SqlitePartCostDataService>();
builder.Services.AddScoped<IFeatureDataService, SqliteFeatureDataService>();
builder.Services.AddScoped<IPartPricingService, SqlitePartPricingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
}
else
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  try
  {
    var context = services.GetRequiredService<ApplicationDbContext>();
    //                    context.Database.Migrate();
    context.Database.EnsureCreated();
    SeedData.Initialize(services);
  }
  catch (Exception ex)
  {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
  }
}
app.Run();

public partial class Program { }
