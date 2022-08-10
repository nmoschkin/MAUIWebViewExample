
using WebViewHostExample.Controls;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

#if ANDROID
using WebViewHostExample.Platforms.Droid.Renderers;
#endif

#if IOS
using WebViewHostExample.Platforms.iOS.Renderers;
#endif

namespace WebViewHostExample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

#if ANDROID
        AppCenter.Start("0c4de546-408d-4b4e-b037-49eab95fd874", typeof(Analytics), typeof(Crashes));
#endif

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
            });
            ;


        return builder.Build();
    }
}
