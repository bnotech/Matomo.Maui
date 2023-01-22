using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Matomo.Maui;

public class ActionBuffer
{
    string baseParameters;
    List<string> inbox = new List<string>();
    List<string> outbox = new List<string>();
    SimpleStorage storage;

    public ActionBuffer(NameValueCollection baseParameters, SimpleStorage storage)
    {
        this.baseParameters = $"?rec=1&apiv=1&{baseParameters}&";
        this.storage = storage;

        inbox = storage.Get<List<string>>("actions_inbox", new List<string>());
        outbox = storage.Get<List<string>>("actions_outbox", new List<string>());
    }

    public int Count { get { return inbox.Count + outbox.Count; } }

    public void Add(NameValueCollection parameters)
    {
        if (OptOut)
            return;

        lock (inbox)
        {
            inbox.Add(baseParameters + parameters);
            storage.Put<List<string>>("actions_inbox", inbox);
        }
    }

    public string CreateOutbox()
    {
        lock (outbox) lock (inbox)
            {
                outbox.AddRange(inbox);
                if (outbox.Count == 0)
                    return "";

                inbox.Clear();
                storage.Put<List<string>>("actions_inbox", inbox);
                storage.Put<List<string>>("actions_outbox", outbox);
            }

        var data = new Dictionary<string, object>();
        data["requests"] = outbox;
        return JsonConvert.SerializeObject(data);
    }

    public void ClearOutbox()
    {
        lock (outbox)
        {
            outbox.Clear();
            storage.Put<List<string>>("actions_outbox", outbox);
        }
    }

    public void Clear()
    {
        lock (outbox) // NOTE we atain a lock for both objects before clearing.
            lock (inbox)
            {
                inbox.Clear();
                outbox.Clear();
                storage.Put<List<string>>("actions_inbox", inbox);
                storage.Put<List<string>>("actions_outbox", outbox);
            }
    }

    public bool OptOut
    {
        get => storage.Get<bool>("opt_out", false);
        set => storage.Put<bool>("opt_out", value);
    }
}