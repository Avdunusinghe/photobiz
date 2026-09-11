using Photobiz.PortfolioSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<PortfolioApiClient>(client =>
{
    var baseUrl = builder.Configuration["PhotobizApi:BaseUrl"]
        ?? throw new InvalidOperationException("PhotobizApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
