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

            // Load configuration files into the configuration manager.
            LoadConfiguration(builder.Configuration, "appsettings.json");
            LoadConfiguration(builder.Configuration, "appsettings.Development.json");

            // Register services with the dependency injection container.
            builder.Services.AddJwtAuth(builder.Configuration);
            builder.Services.AddIotServices(builder.Configuration);

            // Register the Blazor WebView for the application.
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            // Register MudBlazor services for UI components.
            builder.Services.AddMudServices();

            return builder.Build();
        }

        /// <summary>
        /// Loads the configuration from a JSON file into the provided configuration manager.
        /// </summary>
        /// <param name="configuration">The configuration manager to which the JSON settings will be added.</param>
        /// <param name="configFile">The name of the JSON configuration file to load.</param>
        private static void LoadConfiguration(IConfigurationManager configuration , string configFile)
        {
            try
            {
                // Opens a filestream and add it to the configuration.
                using var settingsStream = FileSystem.OpenAppPackageFileAsync(configFile).GetAwaiter().GetResult();
                configuration.AddJsonStream(settingsStream);
            }
            catch (Exception ex)
            {
                // If the file is not found, the error is ignored.
                Console.WriteLine($"{configFile} was not found - continuing without.");
            }
        }
    }
}
