using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models.Vct.Attributes;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.SdJwtVc.Models.Vct.Models.VctMetadataJsonKeys;

namespace WalletFramework.SdJwtVc.Models.Vct.Models;

/// <summary>
///     Represents the metadata of a specific vc type.
/// </summary>
public class VctMetadata
{
    /// <summary>
    ///     Gets or sets the vct.
    /// </summary>
    public Option<Vct> Vct { get; }
        
    /// <summary>
    ///     Gets or sets the integrity metadaty
    /// </summary>
    public Option<VctName> Name { get; }

    /// <summary>
    ///     Gets or sets the human readable description for the type.
    /// </summary>
    public Option<VctDescription> Description { get; }

    /// <summary>
    ///     Gets or sets the URI of another type that this type extends.
    /// </summary>
    public Option<VctExtends> Extends { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the display information of the vc type in different languages.
    /// </summary>
    public Option<Dictionary<Locale, VctDisplay>> Display { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the claim information of the vc type in different languages.
    /// </summary>
    public Option<ClaimMetadata> Claims { get; }
        
    /// <summary>
    ///     Gets or sets the embedded JSON schema document describing the structure of the credential.
    /// </summary>
    public Option<string> Schema { get; }
        
    /// <summary>
    ///     Gets or sets the URL pointing to a JSON schema document desribing the structure of the credential.
    /// </summary>
    public Option<Uri> SchemaUrl { get; }
        
    /// <summary>
    ///     Gets or sets the integrity metadata for the schema url.
    /// </summary>
    public Option<string> SchemaUrlIntegrity { get; }

    private VctMetadata(
        Option<Vct> vct,
        Option<VctName> name,
        Option<VctDescription> description,
        Option<VctExtends> extends,
        Option<Dictionary<Locale, VctDisplay>> display,
        Option<ClaimMetadata> claims,
        Option<string> schema,
        Option<Uri> schemaUrl)
    {
        Vct = vct;
        Name = name;
        Description = description;
        Extends = extends;
        Display = display;
        Claims = claims;
        Schema = schema;
        SchemaUrl = schemaUrl;
    }
        
    private static VctMetadata Create(
        Option<Vct> vct,
        Option<VctName> name,
        Option<VctDescription> description,
        Option<VctExtends> extends,
        Option<Dictionary<Locale, VctDisplay>> display,
        Option<ClaimMetadata> claims,
        Option<string> schema,
        Option<Uri> schemaUrl
        ) => new(
        vct,
        name,
        description,
        extends,
        display,
        claims,
        schema,
        schemaUrl);
        
    public static Validation<VctMetadata> ValidVctMetadata(JObject json)
    {
        var vct = json.GetByKey(VctJsonName).OnSuccess(Models.Vct.ValidVct).ToOption();
        var name = json.GetByKey(NameJsonName).ToOption().OnSome(VctName.OptionVctName);
        var description = json.GetByKey(NameJsonName).ToOption().OnSome(VctDescription.OptionVctDescription);
        var extends = json.GetByKey(ExtendsJsonName).OnSuccess(VctExtends.ValidVctExtends).ToOption();

        return Valid(Create)
            .Apply(vct)
            .Apply(name)
            .Apply(description)
            .Apply(extends)
            .Apply(Option<Dictionary<Locale, VctDisplay>>.None)
            .Apply(Option<ClaimMetadata>.None)
            .Apply(Option<string>.None)
            .Apply(Option<Uri>.None);
    }
}

public static class VctMetadataJsonKeys
{
    public const string VctJsonName = "vct";
    public const string NameJsonName = "name";
    public const string DescriptionJsonName = "description";
    public const string ExtendsJsonName = "extends";
    public const string DisplayJsonName = "display";
    public const string ClaimsJsonName = "claims";
    public const string SchemaJsonName = "schema";
    public const string SchemaUrlJsonName = "schema_url";
}
