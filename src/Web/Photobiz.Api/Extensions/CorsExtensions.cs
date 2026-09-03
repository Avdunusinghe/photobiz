namespace Photobiz.Api.Extensions
{
    public static class CorsExtensions
    {
        public const string PolicyName = "Default";

        public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
