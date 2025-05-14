using IotProject.Auth.Services;
using IotProject.BlazorDashboard.Components;

namespace IotProject.BlazorDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddBlazorBootstrap();

            builder.Services.AddJwtAuth(builder.Configuration);

            builder.Services.AddIotServices();

            // To be moved into seperate library
            builder.Services.AddScoped<RoomService>();
            builder.Services.AddHttpClient<RoomService>(options =>
            {
                options.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_URL")! ?? builder.Configuration.GetConnectionString("ApiUrl")!);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
