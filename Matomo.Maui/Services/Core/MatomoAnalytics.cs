using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using Matomo.Maui.Buffers;
using Matomo.Maui.Models;
using Matomo.Maui.Services.Storage;

namespace Matomo.Maui.Services.Core;

public class MatomoAnalytics : IMatomoAnalytics
{
    #region Properties
    
    private string _userAgent;
    public string UserAgent
    {
        get
        {
            if (string.IsNullOrEmpty(_userAgent))
            {
                _userAgent = $"Mozilla/5.0 ({DeviceInfo.Platform} {DeviceInfo.VersionString}; {DeviceInfo.Manufacturer} {DeviceInfo.Model})";
            }
            return _userAgent;
        }
    }

    public bool Verbose { get; set; } = false;

    public int UnsentActions { get { lock (_actions) return _actions.Count; } }

    private List<Dimension> _dimensions;
    public List<Dimension> Dimensions
    {
        get { return _dimensions ??= []; }
        set => _dimensions = value;
    }
    
    public bool OptOut
    {
        get => _actions is { OptOut: true };
        set
        {
            if (_actions != null)
                _actions.OptOut = value;
        }
    }
    
    /// <summary>
    /// The base url used by the app (piwi's url parameter). Default is https://app
    /// </summary>
    public string AppUrl { get; set; } = "https://app";
    
    #endregion
    
    #region Attributes
    
    private readonly string _apiUrl;
    private readonly ActionBuffer _actions;
    private readonly NameValueCollection _pageParameters;

    private readonly HttpClient _httpClient = new HttpClient();
    private readonly Random _random = new Random();
    
    private readonly System.Timers.Timer _timer = new System.Timers.Timer();

    #endregion
    
    public MatomoAnalytics(IConfiguration configuration, ISimpleStorage storage)
    {
        var visitor = GenerateId(16);
        if (storage.HasKey("visitor_id"))
            visitor = storage.Get("visitor_id");
        else
        {
            storage.Put("visitor_id", visitor);
        }
        
        _dimensions = [];
        
        this._apiUrl = $"{configuration["Matomo:ApiUrl"]}/piwik.php";
        var baseParameters = HttpUtility.ParseQueryString(string.Empty);
        baseParameters["idsite"] = configuration["Matomo:SiteId"];
        baseParameters["_id"] = visitor;
        baseParameters["cid"] = visitor;

        AppUrl = configuration["Matomo:SiteUrl"];
        _pageParameters = HttpUtility.ParseQueryString(string.Empty);

        _actions = new ActionBuffer(baseParameters, storage);

        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _timer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
        _timer.Elapsed += async (s, args) => await Dispatch();
        _timer.Start();
    }

    /// <summary>
    /// Update a existing Dimension
    /// </summary>
    /// <param name="name">Dimension</param>
    /// <param name="newValue">New value</param>
    public void UpdateDimension(string name, string newValue)
    {
        Dimensions.First(d => d.Name == name).Value = newValue;
    }

    /// <summary>
    /// Tracks a page visit.
    /// </summary>
    /// <param name="name">page name (eg. "Settings", "Users", etc)</param>
    /// <param name="path">path which led to the page (eg. "/settings/language"), default is "/"</param>
    public void TrackPage(string name, string path = "/")
    {
        Log($"[Page] {name}");

        _pageParameters["pv_id"] = GenerateId(6);
        _pageParameters["url"] = $"{AppUrl}{path}";

        var parameters = CreateParameters();
        parameters["action_name"] = name;

        parameters.Add(_pageParameters);

        lock (_actions)
            _actions.Add(parameters);
    }

    /// <summary>
    /// Tracks an page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">optional event name (eg. song title, file name, etc)</param>
    /// <param name="value">optional event value (eg. position in song, count of manual updates, etc)</param>
    public void TrackPageEvent(string category, string action, string name = null, int? value = null)
    {
        var parameters = CreateEventParemeters(category, action, name, value);
        parameters.Add(_pageParameters);

        lock (_actions)
            _actions.Add(parameters);
    }

    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    public void TrackEvent(string category, string action)
    {
        TrackEvent(category, action, null, null);
    }

    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">event name (eg. song title, file name, etc)</param>
    public void TrackEvent(string category, string action, string name)
    {
        TrackEvent(category, action, name, null);
    }

    /// <summary>
    /// Tracks an non-page related event.
    /// </summary>
    /// <param name="category">event category ("Music", "Video", etc)</param>
    /// <param name="action">event action ("Play", "Pause", etc)</param>
    /// <param name="name">event name (eg. song title, file name, etc)</param>
    /// <param name="value">event value (eg. position in song, count of manual updates, etc)</param>
    public void TrackEvent(string category, string action, string name, int? value)
    {
        var parameters = CreateEventParemeters(category, action, name, value);
        parameters["url"] = AppUrl; // non-page events must at least have the base url 

        lock (_actions)
            _actions.Add(parameters);
    }

    private NameValueCollection CreateEventParemeters(string category, string action, string name, int? value)
    {
        Log($"[Event] category: {category}, action:{action}, name:{name}, value:{value}");
        var parameters = CreateParameters();
        parameters["e_c"] = category;
        parameters["e_a"] = action;
        if (name != null)
            parameters["e_n"] = name;
        if (value != null)
            parameters["e_v"] = value.ToString();
        return parameters;
    }

    /// <summary>
    /// Tracks a search query.
    /// </summary>
    /// <param name="query">search query (eg. "cats")</param>
    /// <param name="resultCount">number of search results displayed to the user</param>
    /// <param name="category">categroy (eg. "cats")</param>
    public void TrackSearch(string query, int resultCount, string category = null)
    {
        Log($"[Search] {query} {resultCount} {category}");
        var parameters = CreateParameters();
        parameters["search"] = query;
        parameters["search_count"] = resultCount.ToString();
        if (category != null)
            parameters["search_cat"] = category;

        parameters.Add(_pageParameters);

        lock (_actions)
            _actions.Add(parameters);
    }

    /// <summary>
    /// Track an App exit. Needed to acuratly time the visibility of last page and triggers an Dispatch().
    /// </summary>
    public async Task LeavingTheApp()
    {
        TrackPage("Close");
        await Dispatch();
    }

    /// <summary>
    /// Dispatches all tracked actions to the Matomo instance
    /// </summary>
    /// <returns>true if successful, else false.</returns>
    public async Task<bool> Dispatch() // TODO run in background: http://arteksoftware.com/backgrounding-with-xamarin-forms/
    {
        var actionsToDispatch = "";
        lock (_actions)
        {
            if (_actions.Count == 0)
                return false;
            actionsToDispatch = _actions.CreateOutbox(); // new action buffer to store tracking infos while we dispatch
        }

        Log($"[Dispatching] {actionsToDispatch}");
        var content = new StringContent(actionsToDispatch, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_apiUrl, content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                lock (_actions)
                    _actions.ClearOutbox();
                return true;
            }

            LogError(response);
        }
        catch (Exception e)
        {
            LogError(e);
            _httpClient.CancelPendingRequests();
        }
        return false;
    }
    
    /// <summary>
    /// Resets the current action queue
    /// </summary>
    public void ClearQueue() => _actions?.Clear();

    private NameValueCollection CreateParameters()
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["rand"] = _random.Next().ToString();
        parameters["cdt"] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString(); // TODO dispatching cdt older thant 24 h needs token_auth in bulk request
        parameters["lang"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        parameters["ua"] = UserAgent;

        foreach (var dimension in Dimensions)
            parameters[$"dimension{dimension.Id}"] = dimension.Value;

        return parameters;
    }

    private void Log(object msg)
    {
        if (Verbose && !_actions.OptOut)
            Console.WriteLine($"[Analytics] {msg}");
    }

    private void LogError(object msg)
    {
        Console.WriteLine($"[Analytics] [Error] {msg}");
    }

    private string GenerateId(int length)
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, length).ToUpper();
    }
}