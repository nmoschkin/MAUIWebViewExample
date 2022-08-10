using Foundation;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using UIKit;

namespace WebViewHostExample
{

    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {

	    protected override MauiApp CreateMauiApp() 
        {
            return MauiProgram.CreateMauiApp();
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            //AppCenter.Start("1518793c-b3c7-47e9-a6e8-3c8447489246", typeof(Analytics), typeof(Crashes));
            return base.FinishedLaunching(application, launchOptions);
        }

    }

}

