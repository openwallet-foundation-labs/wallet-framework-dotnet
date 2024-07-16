using System.Globalization;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredResponse.SdJwt.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredResponse.SdJwt;

public record EncodedSdJwt
{
    private string Value { get; }
    
    public SdJwtDoc Decoded { get; }

    private EncodedSdJwt(string value, SdJwtDoc decoded)
    {
        Value = value;
        Decoded = decoded;
    }

    public override string ToString() => Value;
    
    public static implicit operator string(EncodedSdJwt encodedSdJwt) => encodedSdJwt.Value;

    public static Validation<EncodedSdJwt> ValidEncodedSdJwt(JValue sdJwt)
    {
        var str = sdJwt.ToString(CultureInfo.InvariantCulture);
        try
        {
            var sdJwtDoc = new SdJwtDoc(str);
            return new EncodedSdJwt(str, sdJwtDoc);
        }
        catch (Exception e)
        {
            return new SdJwtParsingError(sdJwt, e);
        }
    }
}
