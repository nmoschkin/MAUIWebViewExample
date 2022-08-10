
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
