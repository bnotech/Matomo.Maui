namespace Matomo.Maui.Services.Storage;

public interface ISimpleStorage
{
    bool HasKey(string key);
    string Get(string key);
    T Get<T>(string key);
    T Get<T>(string key, T defaultValue);
    void Put<T>(string key, T value);
}