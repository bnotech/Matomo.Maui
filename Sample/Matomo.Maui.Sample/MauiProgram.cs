global using Matomo.Maui.Services.Core;
global using Matomo.Maui.Services.Shell;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Matomo.Maui.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var configFileName = "Matomo.Maui.Sample.appsettings.json";
#if DEBUG
		configFileName = "Matomo.Maui.Sample.appsettings.Development.json";
#endif
        
		var a = Assembly.GetExecutingAssembly();
		using var stream = a.GetManifestResourceStream(configFileName);

		var config = new ConfigurationBuilder()
			.AddJsonStream(stream)
			.Build();
		
		var builder = MauiApp.CreateBuilder();
		
		builder.Configuration.AddConfiguration(config);
		
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseMatomo();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

