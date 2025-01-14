namespace Matomo.Maui.Models;

/// <summary>
/// Provides configuration options for the Matomo Analytics Client
/// </summary>
public class MatomoConfig
{
    /// <summary>
    /// The base url used by the app (piwi's url parameter). Default is https://app
    /// </summary>
    public string ApiUrl;
    
    /// <summary>
    /// Site Id
    /// </summary>
    public int SiteId;
}