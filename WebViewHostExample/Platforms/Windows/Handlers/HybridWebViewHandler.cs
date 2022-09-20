using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Handlers;

using WebViewHostExample.Controls;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Maui.Platform;
using Microsoft.UI.Dispatching;
using WinRT;
using Microsoft.UI;

namespace WebViewHostExample.WinUI;

public class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView2>
{
    public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

    const string JavascriptFunction = "function invokeCSharpAction(data){window.chrome.webview.hostObjects.jsBridge.InvokeAction(data);}";

    private JSBridge jsBridgeHandler;

    public HybridWebViewHandler() : base(HybridWebViewMapper)
    {
    }

    private void VirtualView_SourceChanged(object sender, SourceChangedEventArgs e)
    {
        LoadSource(e.Source, PlatformView);
    }

    static SynchronizationContext sync;
    
    protected override WebView2 CreatePlatformView()
    { 
        sync = SynchronizationContext.Current;
        var webView = new WebView2();

        webView.NavigationCompleted += WebView_NavigationCompleted;

        webView.EnsureCoreWebView2Async().AsTask().ContinueWith((t) =>
        {
            sync.Post((o) =>
            {
                var core = webView.CoreWebView2;
                
                
                jsBridgeHandler = new JSBridge(this);
                core.AddHostObjectToScript("jsBridge", jsBridgeHandler);
                
            }, null);
        });

        return webView;
    }

    private void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        var req = new EvaluateJavaScriptAsyncRequest(JavascriptFunction);
        PlatformView.EvaluateJavaScript(req);
    }

    protected override void ConnectHandler(WebView2 platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView.Source != null)
        {
            LoadSource(VirtualView.Source, PlatformView);
        }

        VirtualView.SourceChanged += VirtualView_SourceChanged;
    }

    protected override void DisconnectHandler(WebView2 platformView)
    {
        base.DisconnectHandler(platformView);

        VirtualView.SourceChanged -= VirtualView_SourceChanged;
        VirtualView.Cleanup();

        jsBridgeHandler = null;
    }

    private static void LoadSource(WebViewSource source, WebView2 control)
    {
        try
        {

            if (control.CoreWebView2 == null)
            {
                control.EnsureCoreWebView2Async().AsTask().ContinueWith((t) =>
                {
                   
                    sync.Post((o) =>
                    {
                        if (source is HtmlWebViewSource html)
                        {
                            control.CoreWebView2.NavigateToString(html.Html);
                        }
                        else if (source is UrlWebViewSource url)
                        {
                            control.CoreWebView2.Navigate(url.Url);
                        }
                    }, null);
                });
            }
            else
            {
                if (source is HtmlWebViewSource html)
                {
                    control.CoreWebView2.NavigateToString(html.Html);
                }
                else if (source is UrlWebViewSource url)
                {
                    control.CoreWebView2.Navigate(url.Url);
                }
            }
        }
        catch { }
    }
}

[ComVisible(true)]
public class JSBridge
{
    readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;

    internal JSBridge(HybridWebViewHandler hybridRenderer)
    {
        hybridWebViewRenderer = new WeakReference<HybridWebViewHandler>(hybridRenderer);
    }

    public void InvokeAction(string data)
    {
        HybridWebViewHandler hybridRenderer;

        if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
        {
            hybridRenderer.VirtualView.InvokeAction(data);
        }
    }
}

