using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate.PurposeFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public record Purpose(Locale Locale, string Name)
{
    public static Validation<Purpose> FromJObject(JObject json)
    {
        var locale = json.GetByKey(LocaleJsonKey)
            .OnSuccess(Locale.ValidLocale);

        var name = json.GetByKey(NameJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                if (string.IsNullOrWhiteSpace(value.Value?.ToString()))
                {
                    return new StringIsNullOrWhitespaceError<Purpose>();
                }

                return ValidationFun.Valid(value.Value.ToString());;
            });

        return ValidationFun.Valid(Create)
            .Apply(locale)
            .Apply(name);
    }
    
    public static Validation<Purpose> FromJValue(JValue json)
    {
        if (string.IsNullOrWhiteSpace(json.Value?.ToString()))
        {
            return new StringIsNullOrWhitespaceError<Purpose>();
        }

        var name = ValidationFun.Valid(json.Value.ToString());

        return ValidationFun.Valid(Create)
            .Apply(WalletFramework.Core.Localization.Constants.DefaultLocale)
            .Apply(name);
    }
    
    private static Purpose Create(
        Locale locale,
        string name) =>
        new(locale, name);
}

public static class PurposeFun
{
    public const string LocaleJsonKey = "locale";
    public const string NameJsonKey = "name";
}

public class PurposeConverter : JsonConverter<Purpose[]>
{
    public override void WriteJson(JsonWriter writer, Purpose[]? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override Purpose[] ReadJson(
        JsonReader reader,
        Type objectType,
        Purpose[]? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = ValidationFun.Valid(JToken.Load(reader));
        var purpose = json.OnSuccess(token =>
        {
            return token switch
            {
                JArray => token.ToJArray()
                    .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
                    .OnSuccess(array =>array.Select(Purpose.FromJObject))
                    .OnSuccess(array => array.TraverseAll(x => x)),
                JValue => token.ToJValue()
                    .OnSuccess(Purpose.FromJValue)
                    .OnSuccess(purpose => new List<Purpose> { purpose }.AsEnumerable()),
                _ => new StringIsNullOrWhitespaceError<Purpose>()
            };
        });

        return purpose.Match(
            p => p.ToArray(),
            _ => []
        );
    }
}
