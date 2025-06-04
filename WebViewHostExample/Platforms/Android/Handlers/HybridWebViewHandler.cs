﻿using Android.Graphics;
using Android.Webkit;

using Java.Interop;

using Microsoft.Maui.Handlers;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.Droid.Renderers
{
    public class HybridWebViewHandler : ViewHandler<WebViewHostExample.Controls.IHybridWebView, Android.Webkit.WebView>
    {
        public static PropertyMapper<WebViewHostExample.Controls.IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<WebViewHostExample.Controls.IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

        const string JavascriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";

        private JSBridge jsBridgeHandler;
        static SynchronizationContext sync;

        public HybridWebViewHandler() : base(HybridWebViewMapper)
        {
            sync = SynchronizationContext.Current;
        }

        private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e)
        {
            LoadSource(e.Source, PlatformView);
        }

        protected override Android.Webkit.WebView CreatePlatformView()
        {
            sync = sync ?? SynchronizationContext.Current;

            var webView = new Android.Webkit.WebView(Context);
            jsBridgeHandler = new JSBridge(this);

            webView.Settings.JavaScriptEnabled = true;

            webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
            webView.AddJavascriptInterface(jsBridgeHandler, "jsBridge");

            return webView;
        }

        protected override void ConnectHandler(Android.Webkit.WebView platformView)
        {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null)
            {
                LoadSource(VirtualView.Source, PlatformView);
            }

            VirtualView.SourceChanged += VirtualView_SourceChanged;
            VirtualView.RequestEvaluateJavaScript += VirtualView_RequestEvaluateJavaScript;
        }

        private void VirtualView_RequestEvaluateJavaScript(object sender, EvaluateJavaScriptAsyncRequest e)
        {
            sync.Post((o) => PlatformView.EvaluateJavascript(e.Script, null), null);
        }

        protected override void DisconnectHandler(Android.Webkit.WebView platformView)
        {
            base.DisconnectHandler(platformView);

            VirtualView.SourceChanged -= VirtualView_SourceChanged;
            VirtualView.Cleanup();

            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
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

        internal JSBridge(HybridWebViewHandler hybridRenderer)
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
