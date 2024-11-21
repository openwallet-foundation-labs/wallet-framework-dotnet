using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering.SvgTemplatePropertiesMethodJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

public readonly struct SvgTemplateProperties
{
    /// <summary>
    ///     Gets or sets the orientation for which the SVG template is optimized.
    /// </summary>
    public Option<SvgTemplateOrientation> Orientation { get; }
    
    /// <summary>
    ///     Gets or sets the color scheme for which the SVG template is optimized.
    /// </summary>
    public Option<SvgTemplateColorScheme> ColorScheme { get; }
    
    /// <summary>
    ///     Gets or sets the contrast for which the SVG template is optimized.
    /// </summary>
    public Option<SvgTemplateContrast> Contrast { get; }
    
    private SvgTemplateProperties(
        Option<SvgTemplateOrientation> orientation,
        Option<SvgTemplateColorScheme> colorScheme,
        Option<SvgTemplateContrast> contrast)
    {
        Orientation = orientation;
        ColorScheme = colorScheme;
        Contrast = contrast;
    }
        
    private static SvgTemplateProperties Create(
        Option<SvgTemplateOrientation> orientation,
        Option<SvgTemplateColorScheme> colorScheme,
        Option<SvgTemplateContrast> contrast
    ) => new(
        orientation,
        colorScheme,
        contrast);
        
    public static Validation<SvgTemplateProperties> ValidSvgTemplateProperties(JToken json)
    {
        var orientation = json.GetByKey(OrientationJsonName).OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (!Enum.TryParse(str, true, out SvgTemplateOrientation orientation))
                {
                    return new EnumCanNotBeParsedError<SvgTemplateOrientation>(str);
                }

                return Valid(orientation);
            })
            .ToOption();
        
        var colorScheme = json.GetByKey(ColorSchemeJsonName).OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (!Enum.TryParse(str, true, out SvgTemplateColorScheme colorScheme))
                {
                    return new EnumCanNotBeParsedError<SvgTemplateColorScheme>(str);
                }

                return Valid(colorScheme);
            })
            .ToOption();
        
        var contrast = json.GetByKey(ContrastJsonName).OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (!Enum.TryParse(str, true, out SvgTemplateContrast contrast))
                {
                    return new EnumCanNotBeParsedError<SvgTemplateContrast>(str);
                }

                return Valid(contrast);
            })
            .ToOption();

        return Valid(Create)
            .Apply(orientation)
            .Apply(colorScheme)
            .Apply(contrast);
    }
}

public static class SvgTemplatePropertiesMethodJsonKeys
{
    public const string OrientationJsonName = "orientation";
    public const string ColorSchemeJsonName = "color_scheme";
    public const string ContrastJsonName = "contrast";
}
