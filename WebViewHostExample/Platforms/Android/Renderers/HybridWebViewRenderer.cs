using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.Graphics;
using Android.Webkit;
using Java.Interop;

using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.Droid.Renderers
{
    public class HybridWebViewRenderer : VisualElementRenderer<HybridWebView>
    {
        const string JavascriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";
        Context _context;
        Android.Webkit.WebView control;

        public Android.Webkit.WebView Control
        {
            get => control;
            set
            {
                if (!(value is null) && !value.Equals(control))
                {
                    control = value;
                    base.AddView(value);
                }
            }
        }

        public HybridWebViewRenderer(Context context) : base(context)
        {
            _context = context;            
        }

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var webView = new Android.Webkit.WebView(_context);

                webView.Settings.JavaScriptEnabled = true;
                webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
                Control = webView;
            }
            if (e.OldElement != null)
            {
                Control.RemoveJavascriptInterface("jsBridge");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.SourceChanged -= OnSourceChanged;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {
                var hybridWebView = e.NewElement as HybridWebView;

                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");

                LoadSource(Element.Source);

                hybridWebView.SourceChanged += OnSourceChanged;

                //Control.LoadUrl($"file:///android_asset/Content/{Element.Uri}");
            }
        }

        private void OnSourceChanged(object sender, SourceChangedEventArgs e)
        {
            LoadSource(e.Source);
        }

        private void LoadSource(WebViewSource source)
        {
            try
            {
                if (source is HtmlWebViewSource html)
                {
                    Control.LoadDataWithBaseURL(html.BaseUrl, html.Html, null, "charset=UTF-8", null);
                }
                else if (source is UrlWebViewSource url)
                {
                    Control.LoadUrl(url.Url);
                }
            }
            catch { }
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
        readonly WeakReference<HybridWebViewRenderer> hybridWebViewRenderer;

        public JSBridge(HybridWebViewRenderer hybridRenderer)
        {
            hybridWebViewRenderer = new WeakReference<HybridWebViewRenderer>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            HybridWebViewRenderer hybridRenderer;

            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
            {
                hybridRenderer.Element.InvokeAction(data);
            }
        }
    }
}
