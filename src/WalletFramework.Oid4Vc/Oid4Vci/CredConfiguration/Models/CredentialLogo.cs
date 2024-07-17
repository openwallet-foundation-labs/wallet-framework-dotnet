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
    public Option<Uri> Uri { get; }
    
    private CredentialLogo(Option<string> altText, Option<Uri> uri)
    {
        AltText = altText;
        Uri = uri;
    }

    public static Option<CredentialLogo> OptionalCredentialLogo(JToken logo)
    {
        var altText = logo.GetByKey(AltTextJsonKey).ToOption().OnSome(text =>
        {
            var str = text.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return Option<string>.None;

            return str;
        });
        
        var imageUri = logo.GetByKey(UriJsonKey).ToOption().OnSome(uri =>
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
            return Option<CredentialLogo>.None;
        
        return new CredentialLogo(altText, imageUri);
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

        logo.Uri.IfSome(uri =>
        {
            result.Add(UriJsonKey, uri.ToStringWithoutTrail());
        });

        return result;
    }
}
