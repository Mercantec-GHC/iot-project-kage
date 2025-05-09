using IotProject.RazorShared.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Config
    {
        public static IServiceCollection AddIotServices(this IServiceCollection services)
        {
            services.AddScoped<IDeviceService, IotDeviceService>();
            return services;
        }
    }
}
