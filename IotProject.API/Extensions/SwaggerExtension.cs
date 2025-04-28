using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerExtension
    {
        // Extends the IServiceCollection with new function to add Authorization with Swagger.
        public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
        {
            // Configure Swagger with JWT Authorization.
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "IotProject.API", Version = "v1" });
                options.MapType<DateTime>(() => new OpenApiSchema { Type = "string", Format = "date" });
                options.MapType<DateTime?>(() => new OpenApiSchema { Type = "string", Format = "date" });

                // Configures the Swagger Login screen.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insert your token here. A token can be obtained from \"/auth/login\" using a username and a password or from \"/auth/refresh\" using a short lived token issued by the server.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                // Adds the security requirement to Swagger.
                options.AddSecurityRequirement(new OpenApiSecurityRequirement { {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { } }
                });
            });
            return services;
        }
    }
}