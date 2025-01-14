namespace Matomo.Maui.Sample;

public partial class MainPage : ContentPage
{
	private IMatomoAnalytics _matomo;
	private int _count = 0;

	public MainPage()
	{
		InitializeComponent();
		
		HandlerChanged += OnHandlerChanged;
	}
	
	void OnHandlerChanged(object sender, EventArgs e)
	{
		_matomo = Handler?.MauiContext?.Services.GetService<IMatomoAnalytics>();
	}
	
	private void OnCounterClicked(object sender, EventArgs e)
	{
		_count++;

		if (_count == 1)
			CounterBtn.Text = $"Clicked {_count} time";
		else
			CounterBtn.Text = $"Clicked {_count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
		_matomo?.TrackPageEvent(category: "Button", action: "Counter Clicked", value: _count);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		if (_matomo == null) return;
		_matomo.TrackPage(name: "Home", path: "/");
		_matomo.TrackPageEvent(category: "Page", action: "Appearing");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_matomo?.TrackPageEvent(category: "Page", action: "Disappearing");
	}
}


