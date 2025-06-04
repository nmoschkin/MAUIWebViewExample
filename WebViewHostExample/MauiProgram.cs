
using WebViewHostExample.Controls;

#if ANDROID
using WebViewHostExample.Platforms.Droid.Renderers;
#endif

#if IOS
using WebViewHostExample.Platforms.iOS.Renderers;
#endif

#if WINDOWS
using WebViewHostExample.WinUI;
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
                handlers.AddHandler(typeof(WebViewHostExample.Controls.HybridWebView), typeof(HybridWebViewHandler));
            });
        ;


        return builder.Build();
    }
}
