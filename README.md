
# MAUIWebViewExample
MAUI Custom Handlers For Hybrid Web View

This is a sample MAUI project that implements a custom Hybrid WebView component with JavaScript -> Native interoperability for Android, iOS, and Windows.

___Note:___ Both Windows Support and EvaluateJavaScriptAsync are in the windows-maui-support branch and will be merged, soon.

## Regarding Windows 

According to various articles I read, the current WinUI3 iteration of WebView2 for MAUI is not yet allowing us to invoke _AddHostObjectToScript_. They plan this for a future release.

But, then I remembered it was Windows, so I created a work-around that most certainly emulates the same behavior and achieves the same result, with a somewhat unorthodox solution: by using an __HttpListener__.

## EvaluateJavaScriptAsync

Because people kept asking for it, I have implemented this in all three platforms, as well.


