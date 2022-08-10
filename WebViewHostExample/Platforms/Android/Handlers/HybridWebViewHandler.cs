using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.Graphics;
using Android.Webkit;
using Android.Widget;

using Java.Interop;

using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.Droid.Renderers
{
    public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, Android.Webkit.WebView>
    {
        const string JavascriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";        
        
        public HybridWebViewHandler() : base(HybridWebViewMapper)
        {
        }

        protected override Android.Webkit.WebView CreatePlatformView()
        {
            var webView = new Android.Webkit.WebView(Context);

            webView.Settings.JavaScriptEnabled = true;

            webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
            webView.AddJavascriptInterface(new JSBridge(this), "jsBridge");

            LoadSource(VirtualView.Source, webView);

            return webView;
        }

        protected override void RemoveContainer()
        {
            VirtualView.Cleanup();
            base.RemoveContainer();
        }

        private static void LoadSource(WebViewSource source, Android.Webkit.WebView control)
        {
            try
            {
                if (source is HtmlWebViewSource html)
                {
                    control.LoadDataWithBaseURL(html.BaseUrl, html.Html, null, "charset=UTF-8", null);
                }
                else if (source is UrlWebViewSource url)
                {
                    control.LoadUrl(url.Url);
                }
            }
            catch { }
        }

        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IHybridWebView.Source)] = MapSource
        };
      
        static void MapSource(HybridWebViewHandler handler, IHybridWebView entry)
        {
            var source = entry.Source;
            var control = handler.PlatformView;

            LoadSource(source, control);
        }

    }

    public class JavascriptWebViewClient : WebViewClient
    {
        string _javascript;

        public JavascriptWebViewClient(string javascript)
        {
            _javascript = javascript;
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
            view.EvaluateJavascript(_javascript, null);
        }
    }

    public class JSBridge : Java.Lang.Object
    {
        readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;

        public JSBridge(HybridWebViewHandler hybridRenderer)
        {
            hybridWebViewRenderer = new WeakReference<HybridWebViewHandler>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            HybridWebViewHandler hybridRenderer;

            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
            {
                hybridRenderer.VirtualView.InvokeAction(data);
            }
        }
    }
}
