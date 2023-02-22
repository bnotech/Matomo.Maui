using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace Matomo.Maui;

public class MatomoAnalytics
{
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

    string apiUrl;
    ActionBuffer actions;
    NameValueCollection baseParameters;
    NameValueCollection pageParameters;

    HttpClient httpClient = new HttpClient();
    Random random = new Random();
    SimpleStorage storage = SimpleStorage.Instance;

    System.Timers.Timer timer = new System.Timers.Timer();

    public MatomoAnalytics(string apiUrl, int siteId)
    {
        var visitor = GenerateId(16);
        if (storage.HasKey("visitor_id"))
            visitor = storage.Get("visitor_id");
        else
        {
            storage.Put("visitor_id", visitor);
        }

        this.apiUrl = $"{apiUrl}/piwik.php";
        baseParameters = HttpUtility.ParseQueryString(string.Empty);
        baseParameters["idsite"] = siteId.ToString();
        baseParameters["_id"] = visitor;
        baseParameters["cid"] = visitor;

        pageParameters = HttpUtility.ParseQueryString(string.Empty);

        actions = new ActionBuffer(baseParameters, storage);

        httpClient.Timeout = TimeSpan.FromSeconds(30);

        timer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
        timer.Elapsed += async (s, args) => await Dispatch();
        timer.Start();
    }

    public bool Verbose { get; set; } = false;

    public int UnsentActions { get { lock (actions) return actions.Count; } }

    public List<Dimension> Dimensions = new List<Dimension>();

    public void UpdateDimension(string name, string newValue)
    {
        Dimensions.First(d => d.Name == name).Value = newValue;
    }

    /// <summary>
    /// The base url used by the app (piwi's url parameter). Default is http://app
    /// </summary>
    public string AppUrl { get; set; } = "http://app";

    /// <summary>
    /// Tracks a page visit.
    /// </summary>
    /// <param name="name">page name (eg. "Settings", "Users", etc)</param>
    /// <param name="path">path which led to the page (eg. "/settings/language"), default is "/"</param>
    public void TrackPage(string name, string path = "/")
    {
        Log($"[Page] {name}");

        pageParameters["pv_id"] = GenerateId(6);
        pageParameters["url"] = $"{AppUrl}{path}";

        var parameters = CreateParameters();
        parameters["action_name"] = name;

        parameters.Add(pageParameters);

        lock (actions)
            actions.Add(parameters);
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
        parameters.Add(pageParameters);

        lock (actions)
            actions.Add(parameters);
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

        lock (actions)
            actions.Add(parameters);
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

        parameters.Add(pageParameters);

        lock (actions)
            actions.Add(parameters);
    }

    /// <summary>
    /// Track an App exit. Needed to acuratly time the visibility of last page and triggers an Dispatch().
    /// </summary>
    public void LeavingTheApp()
    {
        TrackPage("Close");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Dispatch();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public async Task<bool> Dispatch() // TODO run in background: http://arteksoftware.com/backgrounding-with-xamarin-forms/
    {
        var actionsToDispatch = "";
        lock (actions)
        {
            if (actions.Count == 0)
                return false;
            actionsToDispatch = actions.CreateOutbox(); // new action buffer to store tracking infos while we dispatch
        }

        Log($"[Dispatching] {actionsToDispatch}");
        var content = new StringContent(actionsToDispatch, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(apiUrl, content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                lock (actions)
                    actions.ClearOutbox();
                return true;
            }

            LogError(response);
        }
        catch (Exception e)
        {
            LogError(e);
            httpClient.CancelPendingRequests();
        }
        return false;
    }

    public bool OptOut
    {
        get => actions.OptOut;
        set => actions.OptOut = value;
    }

    public void ClearQueue() => actions.Clear();

    NameValueCollection CreateParameters()
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["rand"] = random.Next().ToString();
        parameters["cdt"] = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString(); // TODO dispatching cdt older thant 24 h needs token_auth in bulk request
        parameters["lang"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        parameters["ua"] = UserAgent;

        foreach (var dimension in Dimensions)
            parameters[$"dimension{dimension.Id}"] = dimension.Value;

        return parameters;
    }

    void Log(object msg)
    {
        if (Verbose && !actions.OptOut)
            Console.WriteLine($"[Analytics] {msg}");
    }

    void LogError(object msg)
    {
        Console.WriteLine($"[Analytics] [Error] {msg}");
    }

    static string GenerateId(int length)
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, length).ToUpper();
    }
}