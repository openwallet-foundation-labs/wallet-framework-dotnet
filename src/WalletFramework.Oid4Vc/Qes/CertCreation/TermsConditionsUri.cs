using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Qes.CertCreation;

public readonly struct TermsConditionsUri
{
    public Uri Uri { get; }
    
    private TermsConditionsUri(Uri uri)
    {
        Uri = uri;
    }
        
    private static TermsConditionsUri Create(Uri uri) => new(uri);
        
    public static Validation<TermsConditionsUri> ValidTermsConditionsUri(JValue value)
    {
        if (!Uri.TryCreate(value.ToString(CultureInfo.InvariantCulture), UriKind.Absolute, out var uri))
        {
            return new UriCanNotBeParsedError<TermsConditionsUri>();
        }

        return Valid(Create(uri));
    }
}
