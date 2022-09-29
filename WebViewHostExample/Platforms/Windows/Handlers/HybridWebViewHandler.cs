using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Handlers;

using WebViewHostExample.Controls;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Maui.Platform;
using Microsoft.UI.Dispatching;
using WinRT;
using Microsoft.UI;
using System.Net;
using System.Net.Sockets;
using Microsoft.Maui.Animations;
using System.Text;

namespace WebViewHostExample.WinUI;

internal class HybridSocket
{
    private HttpListener listener;
    private HybridWebViewHandler handler;
    bool token = false;
    
    public HybridSocket(HybridWebViewHandler handler)
    {
        this.handler = handler;
        CreateSocket();
    }

    private void CreateSocket()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:32000/");
    }

    public void StopListening()
    {
        token = false;
    }

    private void SendToNative(string json)
    {
        handler.VirtualView.InvokeAction(json);
    }

    public void Listen()
    {
        var s = listener;
        try
        {
            token = true;
            s.Start();
            
            while (token)
            {                
                HttpListenerContext ctx = listener.GetContext();                
                using HttpListenerResponse resp = ctx.Response;

                    resp.AddHeader("Access-Control-Allow-Origin", "null");
                    resp.AddHeader("Access-Control-Allow-Headers", "content-type");

                    var req = ctx.Request;

                    Stream body = req.InputStream;
                    Encoding encoding = req.ContentEncoding;

                    using (StreamReader reader = new StreamReader(body, encoding))
                    {
                        var json = reader.ReadToEnd();

                        if (ctx.Request.HttpMethod == "POST")
                        {
                            SendToNative(json);
                        }
                    }

                resp.StatusCode = (int)HttpStatusCode.OK;
                resp.StatusDescription = "Status OK";
            }
                       
            CreateSocket();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}

public class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView2>
{
    public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler>(ViewHandler.ViewMapper);

    const string JavascriptFunction = @"function invokeCSharpAction(data)
                {
                    var http = new XMLHttpRequest();
                    var url = 'http://localhost:32000';
                    http.open('POST', url, true);
                    http.setRequestHeader('Content-type', 'application/json');
                    http.send(JSON.stringify(data));
                }";

    //private JSBridge jsBridgeHandler;
    static SynchronizationContext sync;
    private HybridSocket jssocket;

    public HybridWebViewHandler() : base(HybridWebViewMapper)
    {
        sync = SynchronizationContext.Current;
        jssocket = new HybridSocket(this);

        Task.Run(() => jssocket.Listen());
    }

    ~HybridWebViewHandler()
    {
        jssocket.StopListening();
    }

    private void OnWebSourceChanged(object sender, SourceChangedEventArgs e)
    {
        LoadSource(e.Source, PlatformView);
    }

    protected override WebView2 CreatePlatformView()
    { 
        sync = sync ?? SynchronizationContext.Current;
        var webView = new WebView2();

        webView.NavigationCompleted += WebView_NavigationCompleted;

        // In the future this should be simple and possible.

        //webView.EnsureCoreWebView2Async().AsTask().ContinueWith((t) =>
        //{
        //    sync.Post((o) =>
        //    {
        //        var core = webView.CoreWebView2;

        //        jsBridgeHandler = new JSBridge(this);
        //        core.AddHostObjectToScript("jsBridge", jsBridgeHandler);

        //    }, null);
        //});

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

        VirtualView.SourceChanged += OnWebSourceChanged;
        VirtualView.RequestEvaluateJavaScript += VirtualView_RequestEvaluateJavaScript;
    }

    private void VirtualView_RequestEvaluateJavaScript(object sender, EvaluateJavaScriptAsyncRequest e)
    {
        sync.Post((o) => PlatformView.EvaluateJavaScript(e), null);
    }

    protected override void DisconnectHandler(WebView2 platformView)
    {
        base.DisconnectHandler(platformView);

        VirtualView.SourceChanged -= OnWebSourceChanged;
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
                    sync.Post((o) => LoadSource(source, control), null);
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

