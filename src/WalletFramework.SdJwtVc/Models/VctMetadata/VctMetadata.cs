using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Integrity;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadataJsonKeys;

namespace WalletFramework.SdJwtVc.Models.VctMetadata;

/// <summary>
///     Represents the metadata of a specific vc type.
/// </summary>
public readonly struct VctMetadata
{
    /// <summary>
    ///     Gets or sets the vct.
    /// </summary>
    public Vct Vct { get; }
        
    /// <summary>
    ///     Gets or sets the integrity metadaty
    /// </summary>
    public Option<string> Name { get; }

    /// <summary>
    ///     Gets or sets the human readable description for the type.
    /// </summary>
    public Option<string> Description { get; }

    /// <summary>
    ///     Gets or sets the URI of another type that this type extends.
    /// </summary>
    public Option<IntegrityUri> Extends { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the display information of the vc type in different languages.
    /// </summary>
    public Option<Dictionary<Locale, VctMetadataDisplay>> Display { get; }
        
    /// <summary>
    ///     Gets or sets the dictionary representing the claim information of the vc type in different languages.
    /// </summary>
    public Option<VctMetadataClaim[]> Claims { get; }
        
    /// <summary>
    ///     Gets or sets the embedded JSON schema document describing the structure of the credential.
    /// </summary>
    public Option<string> Schema { get; }
        
    /// <summary>
    ///     Gets or sets the URL pointing to a JSON schema document desribing the structure of the credential.
    /// </summary>
    public Option<IntegrityUri> SchemaUrl { get; }
    
    private VctMetadata(
        Vct vct,
        Option<string> name,
        Option<string> description,
        Option<IntegrityUri> extends,
        Option<Dictionary<Locale, VctMetadataDisplay>> display,
        Option<VctMetadataClaim[]> claims,
        Option<string> schema,
        Option<IntegrityUri> schemaUrl)
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
        Vct vct,
        Option<string> name,
        Option<string> description,
        Option<IntegrityUri> extends,
        Option<Dictionary<Locale, VctMetadataDisplay>> display,
        Option<VctMetadataClaim[]> claims,
        Option<string> schema,
        Option<IntegrityUri> schemaUrl
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
        var vct = json.GetByKey(VctJsonName).OnSuccess(Vct.ValidVct);
        var name = json
            .GetByKey(NameJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<VctMetadataDisplay>();
                }

                return Valid(str);
            })
            .ToOption();
        
        var description = json
            .GetByKey(DescriptionJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<VctMetadataDisplay>();
                }

                return Valid(str);
            })
            .ToOption();

        var extends = json
            .GetByKey(ExtendsJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(extendsValue =>
            {
                var integrity = json.GetByKey(ExtendsIntegrityJsonName)
                    .OnSuccess(token => token.ToJValue())
                    .OnSuccess(value => value.ToString(CultureInfo.InvariantCulture))
                    .ToOption();
                return IntegrityUri.ValidIntegrityUri(extendsValue.ToString(CultureInfo.InvariantCulture), integrity);
            })
            .ToOption();

        var display = json
            .GetByKey(DisplayJsonName)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(obj => obj.ToValidDictionaryAll(Locale.ValidLocale, VctMetadataDisplay.ValidVctMetadataDisplay))
            .ToOption();

        var claims = json
            .GetByKey(ClaimsJsonName)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(arr => arr.Select(VctMetadataClaim.ValidVctMetadataClaim).Select(x => x.UnwrapOrThrow()).ToArray())
            .ToOption();
        
        var schema = json
            .GetByKey(SchemaJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<VctMetadataDisplay>();
                }

                return Valid(str);
            })
            .ToOption();
        
        var schemaUrl = json
            .GetByKey(SchemaUrlJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(extendsValue =>
            {
                var integrity = json.GetByKey(SchemaUrlIntegrityJsonName)
                    .OnSuccess(token => token.ToJValue())
                    .OnSuccess(value => value.ToString(CultureInfo.InvariantCulture))
                    .ToOption();
                return IntegrityUri.ValidIntegrityUri(extendsValue.ToString(CultureInfo.InvariantCulture), integrity);
            })
            .ToOption();
        
        return Valid(Create)
            .Apply(vct)
            .Apply(name)
            .Apply(description)
            .Apply(extends)
            .Apply(display)
            .Apply(claims)
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
    public const string ExtendsIntegrityJsonName = "extends#integrity";
    public const string DisplayJsonName = "display";
    public const string ClaimsJsonName = "claims";
    public const string SchemaJsonName = "schema";
    public const string SchemaUrlJsonName = "schema_url";
    public const string SchemaUrlIntegrityJsonName = "schema_url#integrity";
}
