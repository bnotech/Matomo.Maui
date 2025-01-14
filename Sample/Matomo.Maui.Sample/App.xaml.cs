using Matomo.Maui.Models;

namespace Matomo.Maui.Sample;

public partial class App : Application
{
	private IMatomoAnalytics _matomo;
	
	public App(IMatomoAnalytics matomo)
	{
		_matomo = matomo;
		_matomo.Verbose = true;
		_matomo.OptOut = false;
		_matomo.Dimensions.Add(new Dimension(id: 1, name: "AppVersion", currentValue: AppInfo.VersionString));
		
		InitializeComponent();

		MainPage = new AppShell();
	}

	protected override async void OnSleep()
	{
		base.OnSleep();

		try
		{
			await _matomo.LeavingTheApp();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.Message);
		}
	}
}

