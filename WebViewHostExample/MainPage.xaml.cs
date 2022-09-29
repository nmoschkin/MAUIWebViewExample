using WebViewHostExample.ViewModels;

namespace WebViewHostExample;

public partial class MainPage : ContentPage
{
	int count = 0;
    MainPageViewModel vm;

	public MainPage()
	{
		InitializeComponent();

        vm = new MainPageViewModel();
        MyWebView.BindingContext = vm;

        MyWebView.JavaScriptAction += MyWebView_JavaScriptAction;
	}

    protected override void OnParentSet()
    {
        base.OnParentSet();
        vm.Source = new HtmlWebViewSource() { Html = htmlSource };
    }

    private void MyWebView_JavaScriptAction(object sender, Controls.JavaScriptActionEventArgs e)
    {
		Dispatcher.Dispatch(() =>
		{
            ChangeLabel.Text = "The Web Button Was Clicked! Count: " + e.Payload;
        });
    }

    string htmlSource = @"
<html>
<head></head>
<body>

<script>
    var counter = 1;
    function buttonClicked(e) {		
		invokeCSharpAction(counter++);
    }

    function nativeDemand(data) {
         var el = document.getElementById('webtext');
         el.innerHTML = data;
    }

</script>

<div style='display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%'>
<h2 style='font-family: script'><i>Fancy Web Title</i></h2>
<button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='javascript:buttonClicked(event)'>Click Me!</button>
<div id='webtext' style='font-family: script'><b>This web text will change when you push the native button.</b></div>
</div>
</html>
";

    private async void EvalButton_Clicked(object sender, EventArgs e)
    {
        await MyWebView.EvaluateJavaScriptAsync(new EvaluateJavaScriptAsyncRequest("nativeDemand('" + ChangeText.Text + "')")); 
    }
}

