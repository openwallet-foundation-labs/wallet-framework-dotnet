using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering.RenderingMetadataJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

/// <summary>
///     Represents the rendering information of a specific vc type.
/// </summary>
public readonly struct RenderingMetadata
{
    /// <summary>
    ///     Gets or sets metadata for the simple rendering method
    /// </summary>
    public Option<SimpleRenderingMethod> Simple { get; }
    
    /// <summary>
    ///     Gets or sets metadata for the svg templates rendering method
    /// </summary>
    public Option<SvgTemplatesRenderingMethod[]> SvgTemplates { get; }
    
    private RenderingMetadata(
        Option<SimpleRenderingMethod> simple,
        Option<SvgTemplatesRenderingMethod[]> svgTemplates)
    {
        Simple = simple;
        SvgTemplates = svgTemplates;
    }
        
    private static RenderingMetadata Create(
        Option<SimpleRenderingMethod> simple,
        Option<SvgTemplatesRenderingMethod[]> svgTemplates
    ) => new(
        simple,
        svgTemplates);
        
    public static Validation<RenderingMetadata> ValidRenderingMetadata(JObject json)
    {
        var simple = json
            .GetByKey(SimpleJsonName)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(SimpleRenderingMethod.ValidSimpleRenderingMethod)
            .ToOption();
        
        var svgTemplates = json
            .GetByKey(SvgTemplatesJsonName)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(arr => arr.Select(SvgTemplatesRenderingMethod.ValidSvgTemplatesRenderingMethod).Select(x => x.UnwrapOrThrow()).ToArray())
            .ToOption();
        
        return Valid(Create)
            .Apply(simple)
            .Apply(svgTemplates);
    }
}

public static class RenderingMetadataJsonKeys
{
    public const string SimpleJsonName = "simple";
    public const string SvgTemplatesJsonName = "svg_templates";
}
