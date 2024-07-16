using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<CredentialIssuerId>))]
public readonly struct CredentialIssuerId
{
    private Uri Value { get; }
    
    private CredentialIssuerId(Uri value) => Value = value;

    public override string ToString() => Value.ToStringWithoutTrail();
    
    public static implicit operator Uri(CredentialIssuerId credentialIssuerId) => credentialIssuerId.Value;

    public static Validation<CredentialIssuerId> ValidCredentialIssuerId(JToken credentialIssuer) => credentialIssuer.ToJValue().OnSuccess(value =>
    {
        try
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri(str);
            return new CredentialIssuerId(uri);
        }
        catch (Exception e)
        {
            return new CredentialIssuerIdError(credentialIssuer.ToString(), e).ToInvalid<CredentialIssuerId>();
        }
    });
}
