using Matomo.Maui.Models;

namespace Matomo.Maui.Services.Core;

public interface IMatomoAnalytics
{
    /// <summary>
    /// Custom UserAgent based on Device and Platform
    /// </summary>
    string UserAgent { get; }
    /// <summary>
    /// Log Matomo tracking to Console
    /// </summary>
    bool Verbose { get; set; }
    /// <summary>
    /// Number of actions that were not yet dispatched to Matomo
    /// </summary>
    int UnsentActions { get; }
    /// <summary>
    /// Custom Dimensions you can assign any custom data to your visitors or actions <see href="https://matomo.org/guide/reporting-tools/custom-dimensions/" />
    /// </summary>
    List<Dimension> Dimensions { get; set; }
    /// <summary>
    /// The base url used by the app (piwi's url parameter). Default is https://app
    /// </summary>
    string AppUrl { get; set; }
    /// <summary>
    /// Indicates if the user has opted-out of tracking.
    /// </summary>
    bool OptOut { get; set; }
    
    /// <summary>
    /// Update a existing Dimension
    /// </summary>
    /// <param name="name">Dimension</param>
    /// <param name="newValue">New value</param>
    void UpdateDimension(string name, string newValue);
    
    /// <summary>
    /// Tracks a page visit.
    /// </summary>
    /// <param name="name">page name (eg. "Settings", "Users", etc)</param>
    /// <param name="path">path which led to the page (eg. "/settings/language"), default is "/"</param>
    void TrackPage(string name, string path = "/");
    
    /// <summary>
    /// Tracks an page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">optional event name (eg. song title, file name, etc)</param>
    /// <param name="value">optional event value (eg. position in song, count of manual updates, etc)</param>
    void TrackPageEvent(string category, string action, string name = null, int? value = null);
    
    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    void TrackEvent(string category, string action);
    
    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">event name (eg. song title, file name, etc)</param>
    void TrackEvent(string category, string action, string name);
    
    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">event name (eg. song title, file name, etc)</param>
    /// <param name="value">event value (eg. position in song, count of manual updates, etc)</param>
    void TrackEvent(string category, string action, string name, int? value);
    
    /// <summary>
    /// Tracks a search query.
    /// </summary>
    /// <param name="query">search query (eg. "cats")</param>
    /// <param name="resultCount">number of search results displayed to the user</param>
    /// <param name="category">categroy (eg. "cats")</param>
    void TrackSearch(string query, int resultCount, string category = null);
    
    /// <summary>
    /// Track an App exit. Needed to accurately time the visibility of last page and triggers an Dispatch().
    /// </summary>
    Task LeavingTheApp();
    
    /// <summary>
    /// Dispatches all tracked actions to the Matomo instance
    /// </summary>
    /// <returns>true if successful, else false.</returns>
    Task<bool> Dispatch();
    
    /// <summary>
    /// Resets the current action queue
    /// </summary>
    void ClearQueue();
}