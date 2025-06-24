using Microsoft.Extensions.Logging;
using bezorgapp.Services;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;

namespace bezorgapp
{
    // MauiProgram.cs
    // Deze klasse configureert en bouwt de MAUI-applicatie.
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>() // Stel de hoofdapplicatie in
                .UseMauiCommunityToolkit() // Voeg CommunityToolkit functionaliteit toe
                .UseBarcodeReader() // Voeg barcode scanner functionaliteit toe
                .ConfigureFonts(fonts =>
                {
                    // Voeg lettertypen toe aan de app
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            
            // Registreer ApiService als singleton voor dependency injection
            builder.Services.AddSingleton<ApiService>();

#if DEBUG
            // Voeg logging toe in debug-modus
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
