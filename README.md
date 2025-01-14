# Matomo.Maui

This library provides [Matomo](https://matomo.org) Tracking for .NET MAUI Apps.

## Status

[![Continuous Integration](https://github.com/bnotech/Matomo.Maui/actions/workflows/ci.yml/badge.svg)](https://github.com/bnotech/Matomo.Maui/actions/workflows/ci.yml) [![NuGet](https://img.shields.io/nuget/v/Matomo.Maui.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Matomo.Maui/)

## Releases

This library is published for .NET 8.0 and above.

## Documentation

You can find a reference documentation [here](https://bnotech.github.io/Matomo.Maui/html/index.html).

And a sample project in the `Sample/` folder.

### Getting started

First add the following section to your appsettings.json:

```
{
  // ...
  "Matomo": {
    "ApiUrl": "https://matomo.org",
    "SiteId": 1,
    "SiteUrl": "https://app"
  }
  // ...
}
```

***ApiUrl*** is the Url of your Matomo instance

***SiteId*** is the ID of the Matomo Website you registered for the App

***SiteUrl*** is the Url you registered for the App in the Matomo Website

Now add the Matomo.Maui Nuget Package to your project:

```
dotnet add package Matomo.Maui
```

Or use your IDE for the job. Next we need to setup Matomo in the ***MauiProgram.cs***:

```
// ...

var builder = MauiApp.CreateBuilder();

builder
    .UseMauiApp<App>()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    })
    .UseMatomo(); // <-- Add this line

// ...

return builder.Build();
```

Last but not least setup your app for dispatiching the events to your Matomo Instance in ***App.xaml.cs***:

```
using Matomo.Maui.Models;

namespace Matomo.Maui.Sample;

public partial class App : Application
{
	private IMatomoAnalytics _matomo;
	
	public App(IMatomoAnalytics matomo)
	{
		_matomo = matomo;
		_matomo.Verbose = true; // recommended for debug use only
		_matomo.OptOut = false; // if you are not hosting your own Matomo instance, please use this in order to provide your user a option to opt-out of tracking.
		_matomo.Dimensions.Add(new Dimension(id: 1, name: "AppVersion", currentValue: AppInfo.VersionString)); // remember to add your custom Dimensions in your Matomo instance first.
		
		InitializeComponent();

		MainPage = new AppShell();
	}

	protected override async void OnSleep()
	{
		base.OnSleep();

		try
		{
			await _matomo.LeavingTheApp(); // <-- Having this call awaited is important for the dispatch to complete.
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.Message);
		}
	}
}
```

You now have access to Matomo via dependency injection, just reference ***IMatomoAnalytics*** in order to have the service injected.

There is also ***IShellHelper*** that helps you to get the current path of the current page. However this is optional.

## Credit

This work is based on the work done at [zauberzeug/xamarin.piwik](https://github.com/zauberzeug/xamarin.piwik)

## License

This project retains the [MIT license](https://github.com/bfn-tech/Matomo.Maui/blob/main/LICENSE.md) as per the original project.

## Support

In case of issues with the library feel free to provide feedback via the Issues tab or if you want to support the project feel free to contribute via Pull Request.

If you need assistance getting started with Matomo and .NET MAUI feel free to [reach out](https://www.bnotech.com/en/contact.html).