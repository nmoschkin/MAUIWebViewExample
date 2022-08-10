using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewHostExample.Controls
{
    public class SourceChangedEventArgs : EventArgs
    {
        public WebViewSource Source
        {
            get;
            private set;
        }

        public SourceChangedEventArgs(WebViewSource source)
        {
            Source = source;
        }
    }

    public class JavaScriptActionEventArgs : EventArgs
    {
        public string Payload { get; private set; }

        public JavaScriptActionEventArgs(string payload)
        {
            Payload = payload;
        }
    }

    public class HybridWebView : ContentView
    {

        public delegate void SourceChangedEvent(object sender, SourceChangedEventArgs e);
        public event SourceChangedEvent SourceChanged;

        public delegate void JavaScriptActionEvent(object sender, JavaScriptActionEventArgs e);
        public event JavaScriptActionEvent JavaScriptAction;

        public HybridWebView()
        {

        }

        public void Refresh()
        {
            if (Source == null) return;
            var s = Source;
            Source = null;
            Source = s;
        }

        public WebViewSource Source
        {
            get { return (WebViewSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
          propertyName: "Source",
          returnType: typeof(WebViewSource),
          declaringType: typeof(HybridWebView),
          defaultValue: new UrlWebViewSource() { Url = "about:blank" },
          propertyChanged: OnSourceChanged);

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as HybridWebView;
            view.SourceChanged?.Invoke(view, new SourceChangedEventArgs(newValue as WebViewSource));
        }

        public void Cleanup()
        {
            JavaScriptAction = null;
        }

        public void InvokeAction(string data)
        {
            JavaScriptAction?.Invoke(this, new JavaScriptActionEventArgs(data));
        }
    }
}
