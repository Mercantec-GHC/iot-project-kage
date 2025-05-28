using ApexCharts;
using IotProject.RazorShared.Services;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Config
    {
        public static IServiceCollection AddIotServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ApiService>();
            services.AddHttpClient<ApiService>(options =>
            {
                options.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_URL")! ?? configuration.GetConnectionString("ApiUrl")!);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });


            services.AddApexCharts();

            return services;
        }
    }
}
