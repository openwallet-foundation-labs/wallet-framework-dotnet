using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Integrity;
using WalletFramework.Core.Json;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering.SvgTemplatesRenderingMethodJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

public readonly struct SvgTemplatesRenderingMethod
{
    /// <summary>
    ///     Gets or sets the URI pointing to the SVG template.
    /// </summary>
    public IntegrityUri Uri { get; }
    
    /// <summary>
    ///     Gets or sets the properties for the SVG template.
    /// </summary>
    public Option<SvgTemplateProperties> Properties { get; }
    
    private SvgTemplatesRenderingMethod(
        IntegrityUri uri,
        Option<SvgTemplateProperties> properties)
    {
        Uri = uri;
        Properties = properties;
    }
        
    private static SvgTemplatesRenderingMethod Create(
        IntegrityUri uri,
        Option<SvgTemplateProperties> properties
    ) => new(
        uri,
        properties);
        
    public static Validation<SvgTemplatesRenderingMethod> ValidSvgTemplatesRenderingMethod(JToken json)
    {
        var uri = json
            .GetByKey(UriJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(extendsValue =>
            {
                var integrity = json.GetByKey(UriIntegrityJsonName)
                    .OnSuccess(token => token.ToJValue())
                    .OnSuccess(value => value.ToString(CultureInfo.InvariantCulture))
                    .ToOption();
                return IntegrityUri.ValidIntegrityUri(extendsValue.ToString(CultureInfo.InvariantCulture), integrity);
            });
            
        var properties = json.GetByKey(PropertiesJsonName).OnSuccess(token => token.ToJObject()).OnSuccess(SvgTemplateProperties.ValidSvgTemplateProperties).ToOption();
        
        return Valid(Create)
            .Apply(uri)
            .Apply(properties);
    }
}

public static class SvgTemplatesRenderingMethodJsonKeys
{
    public const string UriJsonName = "uri";
    public const string UriIntegrityJsonName = "uri#integrity";
    public const string PropertiesJsonName = "properties";
}
