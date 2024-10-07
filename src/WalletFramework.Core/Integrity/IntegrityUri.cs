using System.Globalization;
using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Core.Integrity;

/// <summary>
///     Represents an URI and its integrity metadata.
/// </summary>
public readonly struct IntegrityUri
{
    /// <summary>
    ///     Gets or sets the URI.
    /// </summary>
    public System.Uri Uri { get; }
    
    /// <summary>
    ///     Gets or sets the integrity metadata.
    /// </summary>
    public Option<string> Integrity { get; }
    
    private IntegrityUri(
        System.Uri uri,
        Option<string> integrity)
    {
        Uri = uri;
        Integrity = integrity;
    }
        
    private static IntegrityUri Create(
        System.Uri uri,
        Option<string> integrity
    ) => new(
        uri,
        integrity);
        
    public static Validation<IntegrityUri> ValidLogo(string value, string integrity)
    {
        if (!System.Uri.TryCreate(value, UriKind.Absolute, out System.Uri uri))
        {
            return new UriCanNotBeParsedError<IntegrityUri>();
        }
        
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return new StringIsNullOrWhitespaceError<IntegrityUri>();
        }

        return ValidationFun.Valid(Create(uri, str));
    }
}
