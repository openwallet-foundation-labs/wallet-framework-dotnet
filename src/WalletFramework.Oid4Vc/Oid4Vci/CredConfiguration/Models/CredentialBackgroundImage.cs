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
    ///     Gets the URL of the Background Image image.
    /// </summary>
    public Uri Uri { get; }
    
    private CredentialBackgroundImage(Uri uri)
    {
        Uri = uri;
    }

    public static Option<CredentialBackgroundImage> OptionalCredentialBackgroundImage(JToken json) 
        => json.GetByKey(UriJsonKey).ToOption().Match(
            Some: imageUri => new CredentialBackgroundImage(new Uri(imageUri.ToString())),
            None: () => Option<CredentialBackgroundImage>.None);
}

public static class CredentialBackgroundImageJsonExtensions
{
    public static string UriJsonKey => "uri";
    
    public static JObject EncodeToJson(this CredentialBackgroundImage credentialBackgroundImage)
    {
        var result = new JObject();

        result.Add(UriJsonKey, credentialBackgroundImage.Uri.ToStringWithoutTrail());

        return result;
    }
}
