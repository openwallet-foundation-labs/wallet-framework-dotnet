using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using static WalletFramework.Core.StatusList.StatusListEntryFun;

namespace WalletFramework.Core.StatusList;

public record StatusListEntry(int Idx, string Uri)
{
    public static Validation<StatusListEntry> FromJObject(JObject json)
    {
        var idx = json.GetByKey(IdxJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var idx))
                {
                    return new StringIsNullOrWhitespaceError<StatusListEntry>();
                }

                return ValidationFun.Valid(idx);
            });

        var uri = json.GetByKey(UriJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<StatusListEntry>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            });

        return ValidationFun.Valid(Create)
            .Apply(idx)
            .Apply(uri);
    }
    
    private static StatusListEntry Create(
        int idx,
        string uri) =>
        new(idx, uri);
}

public static class StatusListEntryFun
{
    public const string IdxJsonKey = "idx";
    public const string UriJsonKey = "uri";
}
