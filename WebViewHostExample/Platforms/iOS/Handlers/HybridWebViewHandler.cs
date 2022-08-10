using CoreGraphics;

using Foundation;

using Microsoft.Maui.Handlers;

using WebKit;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.iOS.Renderers
{
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView>
    {
        public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";

        private WKUserContentController userController;
        private JSBridge jsBridgeHandler;

        public HybridWebViewHandler() : base(HybridWebViewMapper)
        {
            VirtualView.SourceChanged += VirtualView_SourceChanged;
        }

        private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e)
        {
            LoadSource(e.Source, PlatformView);
        }

        protected override WKWebView CreatePlatformView()
        {

            jsBridgeHandler = new JSBridge(this);
            userController = new WKUserContentController();

            var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);

            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(jsBridgeHandler, "invokeAction");

            var config = new WKWebViewConfiguration { UserContentController = userController };
            var webView = new WKWebView(CGRect.Empty, config);

            return webView;            
        }

        protected override void ConnectHandler(WKWebView platformView)
        {
            base.ConnectHandler(platformView);

            if (VirtualView.Source != null)
            {
                LoadSource(VirtualView.Source, PlatformView);
            }
        }

        protected override void DisconnectHandler(WKWebView platformView)
        {
            base.DisconnectHandler(platformView);

            VirtualView.SourceChanged -= VirtualView_SourceChanged;

            userController.RemoveAllUserScripts();
            userController.RemoveScriptMessageHandler("invokeAction");
        
            jsBridgeHandler?.Dispose();
            jsBridgeHandler = null;
        }


        private static void LoadSource(WebViewSource source, WKWebView control)
        {
            if (source is HtmlWebViewSource html)
            {
                control.LoadHtmlString(html.Html, new NSUrl(html.BaseUrl ?? "http://localhost", true));
            }
            else if (source is UrlWebViewSource url)
            {
                control.LoadRequest(new NSUrlRequest(new NSUrl(url.Url)));
            }

        }

    }

    public class JSBridge : NSObject, IWKScriptMessageHandler
    {
        readonly WeakReference<ViewHandler<IHybridWebView, WKWebView>> hybridWebViewRenderer;

        internal JSBridge(ViewHandler<IHybridWebView, WKWebView> owner)
        {
            hybridWebViewRenderer = new WeakReference<ViewHandler<IHybridWebView, WKWebView>>(owner);
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (hybridWebViewRenderer.TryGetTarget(out var owner))
            {
                owner.VirtualView?.InvokeAction(message.Body.ToString());
            }
        }
    }


}
