namespace WebViewHostExample;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();

		MyWebView.Source = new HtmlWebViewSource() { Html = htmlSource };

        MyWebView.JavaScriptAction += MyWebView_JavaScriptAction;
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
        let el = document.getElementById('testId');
        if (el) el.innerHTML = 'Firing!';
		invokeCSharpAction(counter++);
    }
</script>

<div style='display: flex; flex-direction: column; justify-content: center; width: 100%'>
<div id='testId'>Test Text</div>
<button style='margin-left: 15px; margin-right: 15px;' id='hereBtn' onclick='javascript:buttonClicked(event)'>Click Me!</button>
</div>
</html>
";

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}

