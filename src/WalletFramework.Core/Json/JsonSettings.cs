using Newtonsoft.Json;

namespace WalletFramework.Core.Json;

public static class JsonSettings
{
    public static JsonSerializerSettings SerializerSettings => new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };
}
