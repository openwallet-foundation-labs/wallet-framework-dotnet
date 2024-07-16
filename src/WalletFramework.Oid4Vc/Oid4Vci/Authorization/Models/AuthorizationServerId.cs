using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<AuthorizationServerId>))]
public readonly struct AuthorizationServerId
{
    private Uri Value { get; }
    
    private AuthorizationServerId(Uri value) => Value = value;
    
    public override string ToString() => Value.ToStringWithoutTrail();
    
    public static implicit operator Uri(AuthorizationServerId authorizationServerId) => authorizationServerId.Value;

    public static Validation<AuthorizationServerId> ValidAuthorizationServerId(JToken authorizationServerId) => authorizationServerId.ToJValue().OnSuccess(value =>
    {
        try
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri(str);
            return new AuthorizationServerId(uri);
        }
        catch (Exception e)
        {
            return new AuthorizationServerIdError(authorizationServerId.ToString(), e).ToInvalid<AuthorizationServerId>();
        }
    });
}
