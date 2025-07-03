using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredResponse.Mdoc;

public record EncodedMdoc
{
    private string Value { get; }
    
    public MdocLib.Mdoc Decoded { get; }

    private EncodedMdoc(string value, MdocLib.Mdoc decoded)
    {
        Value = value;
        Decoded = decoded;
    }

    public override string ToString() => Value;
    
    public static implicit operator string(EncodedMdoc encodedMdoc) => encodedMdoc.Value;

    public static Validation<EncodedMdoc> ValidEncodedMdoc(JValue mdoc)
    {
        var str = mdoc.ToString(CultureInfo.InvariantCulture);

        return MdocLib.Mdoc
            .ValidMdoc(str)
            .Match(
                validMDoc => new EncodedMdoc(str,validMDoc),
                _ => MdocLib.Mdoc.FromIssuerSigned(str).OnSuccess(validIssuerSigned => new EncodedMdoc(str, validIssuerSigned)));
    }
}
