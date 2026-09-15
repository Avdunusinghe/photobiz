using Microsoft.AspNetCore.Diagnostics;
using Photobiz.PortfolioApp.Services;
using WebOptimizer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/site.css");
    pipeline.AddJavaScriptBundle("/js/bundle.js", "js/site.js");
});

var apiBaseUrl = builder.Configuration["PhotobizApi:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration value 'PhotobizApi:BaseUrl' is required.");

builder.Services.AddHttpClient<PortfolioApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<ICurrentSiteProvider, CurrentSiteProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline. A public-facing site should never leak a stack trace, in
// any environment, so the friendly error handler is used in Development too (unlike the default
// template) — the SiteNotFound/Error views are exactly what needs verifying during development.
app.UseExceptionHandler("/error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
