using Photobiz.Api.ExceptionHandling;
using Photobiz.Api.Extensions;
using Photobiz.Api.Middleware;
using Photobiz.Api.Services;
using Photobiz.Application;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure;
using Photobiz.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITenantService, TenantService>();

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
