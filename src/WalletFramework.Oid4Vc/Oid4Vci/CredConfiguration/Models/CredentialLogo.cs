using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialLogoJsonExtensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the Logo for a Credential.
/// </summary>
public record CredentialLogo
{
    /// <summary>
    ///     Gets the alternate text that describes the logo image. This is typically used for accessibility purposes.
    /// </summary>
    public Option<string> AltText { get; }

    /// <summary>
    ///     Gets the URL of the logo image.
    /// </summary>
    public Uri Uri { get; }
    
    private CredentialLogo(Uri uri, Option<string> altText)
    {
        AltText = altText;
        Uri = uri;
    }

    public static Option<CredentialLogo> OptionalCredentialLogo(JToken logo)
    {
        var altText = logo.GetByKey(AltTextJsonKey).ToOption().OnSome(text =>
        {
            var str = text.ToString();
            return string.IsNullOrWhiteSpace(str) ? Option<string>.None : str;
        });
        
        return logo.GetByKey(UriJsonKey).ToOption().Match(
            uri => {
                try
                {
                    return new CredentialLogo(new Uri(uri.ToString()), altText);
                }
                catch (Exception)
                {
                    return Option<CredentialLogo>.None;
                } 
            }, 
            () => Option<CredentialLogo>.None);
    }
}

public static class CredentialLogoJsonExtensions
{
    public const string AltTextJsonKey = "alt_text";
    public static string UriJsonKey => "uri";
    
    public static JObject EncodeToJson(this CredentialLogo logo)
    {
        var result = new JObject();

        logo.AltText.IfSome(altText =>
        {
            result.Add(AltTextJsonKey, altText);
        });

        result.Add(UriJsonKey, logo.Uri.ToStringWithoutTrail());

        return result;
    }
}
