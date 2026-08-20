using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Photobiz.Api.Extensions
{
    public static class OpenApiExtensions
    {
        private const string BearerScheme = "Bearer";

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        public static IServiceCollection AddOpenApiWithJwtBearer(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    var components = document.Components ??= new OpenApiComponents();
                    var securitySchemes = components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    securitySchemes[BearerScheme] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter a JWT bearer token obtained from POST /api/auth/token."
                    };

                    var security = document.Security ??= new List<OpenApiSecurityRequirement>();
                    security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerScheme, document, null)] = []
                    });
                    return Task.CompletedTask;
                });
            });

            return services;
        }

        public static WebApplication MapApiDocumentation(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(BearerScheme));
            }

            return app;
        }
    }
}
