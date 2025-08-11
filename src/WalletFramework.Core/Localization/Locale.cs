using System.ComponentModel;
using System.Globalization;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Errors;

namespace WalletFramework.Core.Localization;

/// <summary>
///     A value type that represent a locale or respectively a language tag. For example
///     ("en-US"). These are based on RFC 4646: https://www.rfc-editor.org/rfc/rfc4646.html.
///     <remarks>Locales are case-sensitive.</remarks>
/// </summary>
[JsonConverter(typeof(LocalCo))]
[TypeConverter(typeof(LocaleTypeConverter))]
public readonly record struct Locale
{
    private CultureInfo Value { get; }

    private Locale(CultureInfo value) => Value = value;
    
    public static implicit operator string(Locale locale) => locale.Value.ToString();

    public CultureInfo AsCultureInfo => Value;

    [JsonConstructor]
    private Locale(string locale)
    {
        var result = ValidLocale(locale).UnwrapOrThrow(new InvalidOperationException("Locale is corrupt"));
        Value = result.Value;
    }
    
    public static Validation<Locale> ValidLocale(string locale)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(locale))
                return new StringIsNullOrWhitespaceError<Locale>();

            var cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            if (cultureInfo.TwoLetterISOLanguageName == "iv")
                return new LocaleError(locale);
            
            return new Locale(cultureInfo);
        }
        catch (Exception e)
        {
            return new LocaleError(locale, e);
        }
    }
    
    public static Validation<Locale> ValidLocale(JToken locale) =>
        from value in locale.ToJValue()
        from result in ValidLocale(value.ToString(CultureInfo.InvariantCulture))
        select result;

    public static Option<Locale> OptionLocale(string locale) => ValidLocale(locale).ToOption();
    
    public static Option<Locale> OptionLocale(JToken locale) => ValidLocale(locale).ToOption();
    
    public static Locale Create(string locale) => new(locale);
    
    public override string ToString() => this;
}

public static class LocaleExtensions
{
    /// <summary>
    ///     Tries to find a match for a given appLocale inside a dictionary.
    ///     If no match is found it will try again with the DefaultLocale ("en").
    ///     If no match for DefaultLocale is found, it will return the first result that is found inside the dictionary.
    /// </summary>
    /// <param name="displays"> Dictionary with locales as keys and display objects as values.</param>
    /// <param name="locale">The locale that should be matched.</param>
    /// <typeparam name="TDisplay">The type of the display object.</typeparam>
    /// <remarks>Dictionary must be not empty otherwise this will throw an exception.</remarks>
    /// <returns>
    ///     The TDisplay that matches the appLocale or one that matches the DefaultLocale or the first locale inside the
    ///     dictionary.
    /// </returns>
    public static TDisplay FindOrDefault<TDisplay>(this IDictionary<Locale, TDisplay> displays, Locale locale)
    {
        var matchedLocale =
            displays
                .Keys
                .Find(x => x.ToString().Contains(locale))
                .IfNone(() => displays
                    .Keys
                    .Find(x => x.ToString().Contains(Constants.DefaultLocale))
                    .IfNone(() => displays.Keys.First()));
        
        return displays[matchedLocale];
    }

    public static bool IsGerman(this Locale locale)
    {
        return locale.ToString().Contains("de");
    }
    
    public static bool IsEnglish(this Locale locale)
    {
        return locale.ToString().Contains("en");
    }
}
