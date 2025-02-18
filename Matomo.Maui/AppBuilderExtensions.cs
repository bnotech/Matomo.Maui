﻿using Matomo.Maui.Services.Core;
using Matomo.Maui.Services.Shell;
using Matomo.Maui.Services.Storage;

namespace Matomo.Maui;

/// <summary>
/// <see cref="MauiAppBuilder"/> Extensions
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Initializes the Matomo Analytics Client
    /// </summary>
    /// <param name="builder"><see cref="MauiAppBuilder"/> generated by <see cref="MauiApp"/> </param>
    /// <returns><see cref="MauiAppBuilder"/> initialized for <see cref="Matomo.Maui"/></returns>
    public static MauiAppBuilder UseMatomo(this MauiAppBuilder builder)
    {
        builder
            .Services
                .AddTransient<IShellHelper, ShellHelper>()
                .AddSingleton<ISimpleStorage, SimpleStorage>()
                .AddSingleton<IMatomoAnalytics, MatomoAnalytics>();

        return builder;
    }
}
