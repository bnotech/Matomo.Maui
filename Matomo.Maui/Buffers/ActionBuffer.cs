using System.Collections.Specialized;
using Newtonsoft.Json;
using Matomo.Maui.Services.Storage;

namespace Matomo.Maui.Buffers;

public class ActionBuffer
{
    private readonly string _baseParameters;
    private readonly List<string> _inbox;
    private readonly List<string> _outbox;
    private readonly ISimpleStorage _storage;

    public ActionBuffer(NameValueCollection baseParameters, ISimpleStorage storage)
    {
        _baseParameters = $"?rec=1&apiv=1&{baseParameters}&";
        _inbox = [];
        _outbox = [];
        _storage = storage;

        _inbox = storage.Get("actions_inbox", new List<string>());
        _outbox = storage.Get("actions_outbox", new List<string>());
    }

    public int Count
    {
        get
        {
            if (_inbox != null && _outbox != null) return _inbox.Count + _outbox.Count;
            return 0;
        }
    }

    public void Add(NameValueCollection parameters)
    {
        if (OptOut)
            return;

        lock (_inbox)
        {
            _inbox.Add(_baseParameters + parameters);
            _storage.Put("actions_inbox", _inbox);
        }
    }

    public string CreateOutbox()
    {
        lock (_outbox) lock (_inbox)
        {
            _outbox.AddRange(_inbox);
            if (_outbox.Count == 0)
                return "";

            _inbox.Clear();
            _storage.Put("actions_inbox", _inbox);
            _storage.Put("actions_outbox", _outbox);
        }

        var data = new Dictionary<string, object>();
        data["requests"] = _outbox;
        return JsonConvert.SerializeObject(data);
    }

    public void ClearOutbox()
    {
        lock (_outbox)
        {
            _outbox.Clear();
            _storage.Put("actions_outbox", _outbox);
        }
    }

    public void Clear()
    {
        lock (_outbox) // NOTE we atain a lock for both objects before clearing.
            lock (_inbox)
            {
                _inbox.Clear();
                _outbox.Clear();
                _storage.Put("actions_inbox", _inbox);
                _storage.Put("actions_outbox", _outbox);
            }
    }

    public bool OptOut
    {
        get => _storage.Get("opt_out", false);
        set => _storage.Put("opt_out", value);
    }
}