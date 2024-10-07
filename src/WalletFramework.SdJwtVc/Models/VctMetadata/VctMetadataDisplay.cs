using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadataDisplayJsonKeys;

namespace WalletFramework.SdJwtVc.Models.VctMetadata;

/// <summary>
///     Represents the visual representations for the vc type.
/// </summary>
public readonly struct VctMetadataDisplay
{
    /// <summary>
    ///     Gets or sets the human readable name for the type.
    /// </summary>
    public Option<string> Name { get; }
        
    /// <summary>
    ///     Gets or sets the human readable name for the type.
    /// </summary>
    public Option<string> Description { get; }
        
    /// <summary>
    ///     Gets or sets the rendering information for the type.
    /// </summary>  
    public Option<RenderingMetadata> Rendering { get; }
        
    private VctMetadataDisplay(
        Option<string> name,
        Option<string> description,
        Option<RenderingMetadata> rendering)
    {
        Name = name;
        Description = description;
        Rendering = rendering;
    }
        
    private static VctMetadataDisplay Create(
        Option<string> name,
        Option<string> description,
        Option<RenderingMetadata> rendering
    ) => new(
        name,
        description,
        rendering);
        
    public static Validation<VctMetadataDisplay> ValidVctMetadataDisplay(JToken json)
    {
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
        
        var rendering = json.GetByKey(RenderingJsonName).OnSuccess(token => token.ToJObject()).OnSuccess(RenderingMetadata.ValidRenderingMetadata).ToOption();
        
        return Valid(Create)
            .Apply(name)
            .Apply(description)
            .Apply(rendering);
    }
}

public static class VctMetadataDisplayJsonKeys
{
    public const string NameJsonName = "name";
    public const string DescriptionJsonName = "description";
    public const string RenderingJsonName = "rendering";
}
