using LanguageExt;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

/// <summary>
///     Represents the rendering information of a specific vc type.
/// </summary>
public class RenderingMetadata
{
    /// <summary>
    ///     Gets or sets metadata for the simple rendering method
    /// </summary>
    public Option<SimpleRenderingMethod> Simple { get; }
    
    /// <summary>
    ///     Gets or sets metadata for the svg templates rendering method
    /// </summary>
    public Option<SvgTemplatesRenderingMethod[]> SvgTemplates { get; }
}
