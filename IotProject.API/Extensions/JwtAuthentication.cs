using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtAuthentication
    {
        // Extends the IServiceCollection with JWT Authentication.
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configures the Default Authentication scheme and adds the JWT Bearer
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var JWT = configuration.GetSection("JWT");
                var Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? JWT["Issuer"];
                var Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? JWT["Audience"];
                var Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? JWT["Secret"];
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret!))
                };
            });
            return services;
        }
    }
}
