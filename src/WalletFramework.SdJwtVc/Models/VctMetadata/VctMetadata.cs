using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models.VctMetadata.Attributes;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadataJsonKeys;

namespace WalletFramework.SdJwtVc.Models.VctMetadata;

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
    public Option<VctMetadataName> Name { get; }

    /// <summary>
    ///     Gets or sets the human readable description for the type.
    /// </summary>
    public Option<VctMetadataDescription> Description { get; }

    /// <summary>
    ///     Gets or sets the URI of another type that this type extends.
    /// </summary>
    public Option<VctMetadataExtends> Extends { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the display information of the vc type in different languages.
    /// </summary>
    public Option<Dictionary<Locale, VctMetadataDisplay>> Display { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the claim information of the vc type in different languages.
    /// </summary>
    public Option<ClaimMetadata> Claims { get; }
        
    /// <summary>
    ///     Gets or sets the embedded JSON schema document describing the structure of the credential.
    /// </summary>
    public Option<VctMetadataSchema> Schema { get; }
        
    /// <summary>
    ///     Gets or sets the URL pointing to a JSON schema document desribing the structure of the credential.
    /// </summary>
    public Option<VctMetadataSchemaUrl> SchemaUrl { get; }
    
    private VctMetadata(
        Option<Vct> vct,
        Option<VctMetadataName> name,
        Option<VctMetadataDescription> description,
        Option<VctMetadataExtends> extends,
        Option<Dictionary<Locale, VctMetadataDisplay>> display,
        Option<ClaimMetadata> claims,
        Option<VctMetadataSchema> schema,
        Option<VctMetadataSchemaUrl> schemaUrl)
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
        Option<VctMetadataName> name,
        Option<VctMetadataDescription> description,
        Option<VctMetadataExtends> extends,
        Option<Dictionary<Locale, VctMetadataDisplay>> display,
        Option<ClaimMetadata> claims,
        Option<VctMetadataSchema> schema,
        Option<VctMetadataSchemaUrl> schemaUrl
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
        var name = json.GetByKey(NameJsonName).ToOption().OnSome(VctMetadataName.OptionVctName);
        var description = json.GetByKey(DescriptionJsonName).ToOption().OnSome(VctMetadataDescription.OptionVctDescription);
        var extends = json.GetByKey(ExtendsJsonName).OnSuccess(VctMetadataExtends.ValidVctMetadataExtends).ToOption();
        
        // var display = 
        // var claims =
        
        var schema = json.GetByKey(SchemaJsonName).ToOption().OnSome(VctMetadataSchema.OptionVctSchemaMetadata);
        var schemaUrl = json.GetByKey(SchemaUrlJsonName).OnSuccess(VctMetadataSchemaUrl.ValidVctMetadataSchemaUrl).ToOption();
        
        return Valid(Create)
            .Apply(vct)
            .Apply(name)
            .Apply(description)
            .Apply(extends)
            .Apply(Option<Dictionary<Locale, VctMetadataDisplay>>.None)
            .Apply(Option<ClaimMetadata>.None)
            .Apply(schema)
            .Apply(schemaUrl);
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
