using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vci.Issuer.Errors;

namespace WalletFramework.Oid4Vci.Issuer.Models;

public readonly struct CredentialIssuerId
{
    private string OriginalString { get; }
    private Uri Value { get; }
    
    private CredentialIssuerId(string originalString, Uri value)
    {
        OriginalString = originalString;
        Value = value;
    }

    public override string ToString() => OriginalString;
    
    public static implicit operator Uri(CredentialIssuerId credentialIssuerId) => credentialIssuerId.Value;

    public static Validation<CredentialIssuerId> ValidCredentialIssuerId(JToken credentialIssuer) => credentialIssuer.ToJValue().OnSuccess(value =>
    {
        try
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri(str);
            return new CredentialIssuerId(str, uri);
        }
        catch (Exception e)
        {
            return new CredentialIssuerIdError(credentialIssuer.ToString(), e).ToInvalid<CredentialIssuerId>();
        }
    });
}
