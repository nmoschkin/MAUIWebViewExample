using Microsoft.Maui.Controls.Compatibility.Hosting;

using WebViewHostExample.Controls;


#if ANDROID
using WebViewHostExample.Platforms.Droid.Renderers;
#endif

#if IOS
using WebViewHostExample.Platforms.iOS.Renderers;
#endif

namespace WebViewHostExample;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        AppCenter.Start("android=0c4de546-408d-4b4e-b037-49eab95fd874;" +
                  "ios=1518793c-b3c7-47e9-a6e8-3c8447489246;" +
                  typeof(Analytics), typeof(Crashes));

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCompatibility()
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewRenderer));
#endif

#if IOS
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewRenderer));
#endif
            });
            ;


        return builder.Build();
    }
}
