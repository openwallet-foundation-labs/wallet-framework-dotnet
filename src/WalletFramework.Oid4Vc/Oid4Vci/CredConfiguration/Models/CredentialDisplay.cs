using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models.Credential;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialName;
using static WalletFramework.Core.Localization.Locale;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialLogo;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialDisplayFun;
using Color = WalletFramework.Core.Colors.Color;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the visual representations for the credential.
/// </summary>
public record CredentialDisplay
{
    /// <summary>
    ///     Gets the background color for the Credential.
    /// </summary>
    public Option<Color> BackgroundColor { get; }

    /// <summary>
    ///     Gets the text color for the Credential.
    /// </summary>
    public Option<Color> TextColor { get; }

    /// <summary>
    ///     Gets the logo associated with this Credential.
    /// </summary>
    public Option<CredentialLogo> Logo { get; }

    /// <summary>
    ///     Gets the name of the Credential.
    /// </summary>
    public Option<CredentialName> Name { get; }

    /// <summary>
    ///     Gets the locale, which represents the specific culture or region.
    /// </summary>
    public Option<Locale> Locale { get; }

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
                .GetByKey(BackgroundColorJsonKey)
                .ToOption()
                .OnSome(color => Color.OptionColor(color.ToString()));

            var textColor = jObject
                .GetByKey(TextColorJsonKey)
                .ToOption()
                .OnSome(color => Color.OptionColor(color.ToString()));

            var name = jObject.GetByKey(NameJsonKey).ToOption().OnSome(OptionalCredentialName);
            var logo = jObject.GetByKey(LogoJsonKey).ToOption().OnSome(OptionalCredentialLogo);
            var locale = jObject.GetByKey(LocaleJsonKey).OnSuccess(ValidLocale).ToOption();

            if (name.IsNone && logo.IsNone && backgroundColor.IsNone && locale.IsNone && textColor.IsNone)
                return Option<CredentialDisplay>.None;

            return new CredentialDisplay(logo, name, backgroundColor, locale, textColor);
        });
}

public static class CredentialDisplayFun
{
    public const string BackgroundColorJsonKey = "background_color";
    public const string LocaleJsonKey = "locale";
    public const string LogoJsonKey = "logo";
    public const string NameJsonKey = "name";
    public const string TextColorJsonKey = "text_color";
    
    public static JObject EncodeToJson(this CredentialDisplay display)
    {
        var result = new JObject();

        display.Logo.IfSome(logo => { result.Add(LogoJsonKey, logo.EncodeToJson()); });

        display.Name.IfSome(name => { result.Add(NameJsonKey, name.ToString()); });

        display.BackgroundColor.IfSome(color => { result.Add(BackgroundColorJsonKey, color.ToString()); });

        display.Locale.IfSome(locale => { result.Add(LocaleJsonKey, locale.ToString()); });

        display.TextColor.IfSome(color => { result.Add(TextColorJsonKey, color.ToString()); });

        return result;
    }
    
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
