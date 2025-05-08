using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace IotProject.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            //builder.Configuration["ConnectionStrings:ApiUrl"] = "http://localhost:5298";

            try
            {
                // Forsøg at hente appsettings.json fra appens pakke
                using var appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
                builder.Configuration.AddJsonStream(appSettingsStream);
            }
            catch (Exception ex)
            {
                // Hvis filen ikke findes eller der sker en fejl, kan du logge fejlen
                Console.WriteLine($"Fejl ved indlæsning af appsettings.json: {ex.Message}");
                throw;
            }

            // Prøv at loade appsettings.Development.json som er optional
            try
            {
                using var devSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.Development.json").GetAwaiter().GetResult();
                builder.Configuration.AddJsonStream(devSettingsStream);
            }
            catch (Exception ex)
            {
                // Hvis den ikke findes, ignoreres fejlen - det er helt acceptabelt hvis filen er optional.
                Console.WriteLine("appsettings.Development.json blev ikke fundet - fortsætter uden den.");
            }

            builder.Services.AddJwtAuth(builder.Configuration);

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddMudServices();

            return builder.Build();
        }
    }
}
