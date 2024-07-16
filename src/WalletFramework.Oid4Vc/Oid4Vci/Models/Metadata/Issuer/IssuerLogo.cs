using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

/// <summary>
///     Represents the Logo of the Issuer.
/// </summary>
public record IssuerLogo
{
    /// <summary>
    ///     Gets the alternate text that describes the logo image. This is typically used for accessibility purposes.
    /// </summary>
    [JsonProperty("alt_text")]
    [JsonConverter(typeof(OptionJsonConverter<string>))]
    public Option<string> AltText { get; }

    /// <summary>
    ///     Gets the URL of the logo image.
    /// </summary>
    [JsonProperty("uri")]
    [JsonConverter(typeof(OptionJsonConverter<Uri>))]
    public Option<Uri> Uri { get; }
    
    private IssuerLogo(
        Option<string> altText,
        Option<Uri> uri)
    {
        AltText = altText;
        Uri = uri;
    }

    public static Option<IssuerLogo> OptionalIssuerLogo(JToken logo) => logo.ToJObject().ToOption().OnSome(jObject =>
    {
        var altText = jObject.GetByKey("alt_text").ToOption().OnSome(text =>
        {
            var str = text.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return Option<string>.None;

            return str;
        });
        
        var imageUri = jObject.GetByKey("uri").ToOption().OnSome(uri =>
        {
            try
            {
                var str = uri.ToString();
                var result = new Uri(str);
                return result;
            }
            catch (Exception)
            {
                return Option<Uri>.None;
            }
        });

        if (altText.IsNone && imageUri.IsNone)
            return Option<IssuerLogo>.None;
            
        return new IssuerLogo(altText, imageUri);
    });
}
