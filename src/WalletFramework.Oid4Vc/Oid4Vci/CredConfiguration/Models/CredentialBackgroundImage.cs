using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialBackgroundImageJsonExtensions;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the Background Image for a Credential.
/// </summary>
public record CredentialBackgroundImage
{
    /// <summary>
    ///     Gets the alternate text that describes the Background Image image. This is typically used for accessibility purposes.
    /// </summary>
    public Option<string> AltText { get; }

    /// <summary>
    ///     Gets the URL of the Background Image image.
    /// </summary>
    public Option<Uri> Uri { get; }
    
    private CredentialBackgroundImage(Option<string> altText, Option<Uri> uri)
    {
        AltText = altText;
        Uri = uri;
    }

    public static Option<CredentialBackgroundImage> OptionalCredentialBackgroundImage(JToken logo)
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
            return Option<CredentialBackgroundImage>.None;
        
        return new CredentialBackgroundImage(altText, imageUri);
    }
}

public static class CredentialBackgroundImageJsonExtensions
{
    public const string AltTextJsonKey = "alt_text";
    public static string UriJsonKey => "uri";
    
    public static JObject EncodeToJson(this CredentialBackgroundImage logo)
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
