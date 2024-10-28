using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Colors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering.SimpleRenderingMethodJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

public readonly struct SimpleRenderingMethod
{
    /// <summary>
    ///     Gets or sets the information about the logo to be displayed for the type.
    /// </summary>
    public Option<Logo> Logo { get; }
    
    /// <summary>
    ///     Gets or sets the background color for the credential.
    /// </summary>
    public Option<Color> BackgroundColor { get; }
    
    /// <summary>
    ///     Gets or sets the text color for the credential.
    /// </summary>
    public Option<Color> TextColor { get; }
    
    private SimpleRenderingMethod(
        Option<Logo> logo,
        Option<Color> backgroundColor,
        Option<Color> textColor)
    {
        Logo = logo;
        BackgroundColor = backgroundColor;
        TextColor = textColor;
    }
        
    private static SimpleRenderingMethod Create(
        Option<Logo> logo,
        Option<Color> backgroundColor,
        Option<Color> textColor
        ) => new(
        logo,
        backgroundColor,
        textColor);
        
    public static Validation<SimpleRenderingMethod> ValidSimpleRenderingMethod(JObject json)
    {
        var logo = json.GetByKey(LogoJsonName).OnSuccess(token => token.ToJObject()).OnSuccess(Rendering.Logo.ValidLogo).ToOption();
        var backgroundColor = json.GetByKey(BackgroundColorJsonName).OnSuccess(token => token.ToString()).OnSuccess(Color.OptionColor);
        var textColor = json.GetByKey(TextColorJsonName).OnSuccess(token => token.ToString()).OnSuccess(Color.OptionColor);

        return Valid(Create)
            .Apply(logo)
            .Apply(backgroundColor)
            .Apply(textColor);
    }
}

public static class SimpleRenderingMethodJsonKeys
{
    public const string LogoJsonName = "logo";
    public const string BackgroundColorJsonName = "background_color";
    public const string TextColorJsonName = "text_color";
}
