using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Foundation;

using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;

using WebKit;

using WebViewHostExample.Controls;

namespace WebViewHostExample.Platforms.iOS.Renderers
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
        WKUserContentController userController;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                userController = new WKUserContentController();
                var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
                userController.AddUserScript(script);
                userController.AddScriptMessageHandler(this, "invokeAction");

                var config = new WKWebViewConfiguration { UserContentController = userController };
                var webView = new WKWebView(Frame, config);
                SetNativeControl(webView);
            }
            if (e.OldElement != null)
            {
                userController.RemoveAllUserScripts();
                userController.RemoveScriptMessageHandler("invokeAction");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.SourceChanged -= OnSourceChanged;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {

                var hybridWebView = e.NewElement as HybridWebView;
                hybridWebView.SourceChanged += OnSourceChanged;

                //string fileName = Path.Combine(NSBundle.MainBundle.BundlePath, string.Format("Content/{0}", Element.Uri));
                LoadSource(Element.Source);
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            Element.InvokeAction(message.Body.ToString());
        }

        private void OnSourceChanged(object sender, SourceChangedEventArgs e)
        {
            LoadSource(e.Source);
        }

        private void LoadSource(WebViewSource source)
        {
            if (source is HtmlWebViewSource html)
            {
                Control.LoadHtmlString(html.Html, new NSUrl(html.BaseUrl, true));
            }
            else if (source is UrlWebViewSource url)
            {
                Control.LoadRequest(new NSUrlRequest(new NSUrl(url.Url)));
            }

        }

    }
}
