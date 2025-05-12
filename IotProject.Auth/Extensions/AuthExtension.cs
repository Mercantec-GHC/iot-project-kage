using Blazored.LocalStorage;
using Blazored.SessionStorage;
using IotProject.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthExtension
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBlazoredLocalStorage();
            services.AddBlazoredSessionStorage();
            services.AddCascadingAuthenticationState();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "dummy";
                options.DefaultChallengeScheme = "dummy";
            }).AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("dummy", options => { });
            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            services.AddScoped<AuthService>();
            services.AddHttpClient<AuthService>(options =>
            {
                options.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_URL")! ?? configuration.GetConnectionString("ApiUrl")!);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            return services;
        }
    }
}
