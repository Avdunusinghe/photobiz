using Photobiz.Api.ExceptionHandling;
using Photobiz.Api.Extensions;
using Photobiz.Application;
using Photobiz.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddGlobalExceptionHandling();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApiWithJwtBearer();
builder.Services.AddConfiguredCors(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseAsync();
await app.SeedDevelopmentDataAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapApiDocumentation();

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
