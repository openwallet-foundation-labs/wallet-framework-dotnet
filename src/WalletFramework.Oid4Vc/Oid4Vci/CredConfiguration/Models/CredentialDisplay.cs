using System.Drawing;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Colors;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models.Credential;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialName;
using static WalletFramework.Core.Localization.Locale;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialLogo;
using Color = WalletFramework.Core.Colors.Color;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the visual representations for the credential.
/// </summary>
public record CredentialDisplay
{
    /// <summary>
    ///     Gets the logo associated with this Credential.
    /// </summary>
    [JsonProperty("logo")]
    [JsonConverter(typeof(OptionJsonConverter<CredentialLogo>))]
    public Option<CredentialLogo> Logo { get; }

    /// <summary>
    ///     Gets the name of the Credential.
    /// </summary>
    [JsonProperty("name")]
    [JsonConverter(typeof(OptionJsonConverter<CredentialName>))]
    public Option<CredentialName> Name { get; }

    /// <summary>
    ///     Gets the background color for the Credential.
    /// </summary>
    [JsonProperty("background_color")]
    [JsonConverter(typeof(OptionJsonConverter<Color>))]
    public Option<Color> BackgroundColor { get; }

    /// <summary>
    ///     Gets the locale, which represents the specific culture or region.
    /// </summary>
    [JsonProperty("locale")]
    [JsonConverter(typeof(OptionJsonConverter<Locale>))]
    public Option<Locale> Locale { get; }

    /// <summary>
    ///     Gets the text color for the Credential.
    /// </summary>
    [JsonProperty("text_color")]
    [JsonConverter(typeof(OptionJsonConverter<Color>))]
    public Option<Color> TextColor { get; }
    
    private CredentialDisplay(
        Option<CredentialLogo> logo,
        Option<CredentialName> name,
        Option<Color> backgroundColor,
        Option<Locale> locale,
        Option<Color> textColor)
    {
        Logo = logo;
        Name = name;
        BackgroundColor = backgroundColor;
        Locale = locale;
        TextColor = textColor;
    }

    public static Option<CredentialDisplay> OptionalCredentialDisplay(JToken display) => display
        .ToJObject()
        .ToOption()
        .OnSome(jObject =>
        {
            var backgroundColor = jObject
                .GetByKey("background_color")
                .ToOption()
                .OnSome(color => Color.OptionColor(color.ToString()));
            
            var textColor = jObject
                .GetByKey("text_color")
                .ToOption()
                .OnSome(color => Color.OptionColor(color.ToString()));
            
            var name = jObject.GetByKey("name").ToOption().OnSome(OptionalCredentialName);
            var logo = jObject.GetByKey("logo").ToOption().OnSome(OptionalCredentialLogo);
            var locale = jObject.GetByKey("locale").OnSuccess(ValidLocale).ToOption();

            if (name.IsNone && logo.IsNone && backgroundColor.IsNone && locale.IsNone && textColor.IsNone)
                return Option<CredentialDisplay>.None;
            
            return new CredentialDisplay(logo, name, backgroundColor, locale, textColor);
        });
}

public static class CredentialDisplayFun
{
    // TODO: Unpure
    public static SdJwtDisplay ToSdJwtDisplay(this CredentialDisplay credentialDisplay)
    {
        var logo = new SdJwtDisplay.SdJwtLogo
        {
            Uri = credentialDisplay.Logo.ToNullable()?.Uri.ToNullable()!,
            AltText = credentialDisplay.Logo.ToNullable()?.AltText.ToNullable()
        };
        
        return new SdJwtDisplay
        {
            Logo = logo,
            Name = credentialDisplay.Name.ToNullable(),
            BackgroundColor = credentialDisplay.BackgroundColor.ToNullable(),
            Locale = credentialDisplay.Locale.ToNullable()?.ToString(),
            TextColor = credentialDisplay.TextColor.ToNullable()
        };
    }
}
