using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Photobiz.Api.ExceptionHandling;
using Photobiz.Api.Extensions;
using Photobiz.Api.Middleware;
using Photobiz.Api.Services;
using Photobiz.Application;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Settings;
using Photobiz.Infrastructure;
using Photobiz.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    // Enums cross the wire as their readable names ("Grid", "Classic", ...) rather than raw
    // integers — matches how they're stored in the database (HasConversion<string>()) and keeps
    // API payloads self-describing for the Angular client.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IPublicUrlProvider, RequestPublicUrlProvider>();

builder.Services.AddGlobalExceptionHandling();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApiWithJwtBearer();
builder.Services.AddConfiguredCors();

var app = builder.Build();

await app.MigrateDatabaseAsync();
await DatabaseSeeder.SeedDevelopmentDataAsync(app.Services, app.Environment);

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapApiDocumentation();

// Serves uploaded tenant files (logos, ...) directly off disk, unauthenticated — an <img src> tag
// can't send a bearer token, and these are meant to be publicly viewable (e.g. on a portfolio
// site) anyway. Placed ahead of auth/tenant-resolution so a matched file short-circuits the
// pipeline before either even runs.
var fileStorageSettings = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;
Directory.CreateDirectory(fileStorageSettings.RootPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileStorageSettings.RootPath),
    RequestPath = fileStorageSettings.PublicPathPrefix
});

// Redirecting an OPTIONS preflight to https breaks CORS entirely (browsers refuse to follow
// a redirect for a preflight request), so only enforce HTTPS outside local development, where
// the SPA talks to this API over plain http and there's no TLS termination to redirect to anyway.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();

// Resolves the request's tenant (from the login body, or from the validated JWT for every other
// endpoint) and points the ambient tenant DbContext at that tenant's database before any
// controller/handler runs. Must run after UseAuthentication() so the JWT claim is available, and
// before MapControllers() so every handler sees the right database.
app.UseMiddleware<TenantSelectionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
