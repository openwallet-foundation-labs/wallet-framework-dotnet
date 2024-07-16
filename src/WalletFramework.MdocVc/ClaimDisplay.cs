using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Localization;

namespace WalletFramework.MdocVc;

public record ClaimDisplay(
    [property: JsonProperty(ClaimDisplayJsonKeys.ClaimName)]
    [property: JsonConverter(typeof(OptionJsonConverter<ClaimName>))]
    Option<ClaimName> Name, 
    [property: JsonProperty(ClaimDisplayJsonKeys.Locale)]
    [property: JsonConverter(typeof(OptionJsonConverter<Locale>))]
    Option<Locale> Locale);

public static class ClaimDisplayJsonKeys
{
    public const string ClaimName = "name";
    public const string Locale = "locale";
}
