using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CoreGraphics;

using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using ObjCRuntime;
using WebKit;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.iOS.Renderers
{
    public class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView>
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
        WKUserContentController userController;
        ScriptMessageHandler msgHandler;

        public HybridWebViewHandler() : base(HybridWebViewMapper)
        {
        }

        protected override WKWebView CreatePlatformView()
        {

            msgHandler = new ScriptMessageHandler(this);
            userController = new WKUserContentController();

            var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);

            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(msgHandler, "invokeAction");

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
            userController.RemoveAllUserScripts();
            userController.RemoveScriptMessageHandler("invokeAction");
            msgHandler?.Dispose();
            msgHandler = null;
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

    public class ScriptMessageHandler : NSObject, IWKScriptMessageHandler
    {

        ViewHandler<IHybridWebView, WKWebView> owner;

        internal ScriptMessageHandler(ViewHandler<IHybridWebView, WKWebView> owner)
        {
            this.owner = owner;
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            owner.VirtualView?.InvokeAction(message.Body.ToString());
        }
    }


}
