using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vci.Authorization.Errors;

namespace WalletFramework.Oid4Vci.Authorization.Models;

public readonly struct AuthorizationServerId
{
    private string OriginalString { get; }
    private Uri Value { get; }
    
    private AuthorizationServerId(string originalString, Uri value)
    {
        OriginalString = originalString;
        Value = value;
    }
    
    public override string ToString() => OriginalString;
    
    public static implicit operator Uri(AuthorizationServerId authorizationServerId) => authorizationServerId.Value;

    public static Validation<AuthorizationServerId> ValidAuthorizationServerId(JToken authorizationServerId) => authorizationServerId.ToJValue().OnSuccess(value =>
    {
        try
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri(str);
            return new AuthorizationServerId(str, uri);
        }
        catch (Exception e)
        {
            return new AuthorizationServerIdError(authorizationServerId.ToString(), e).ToInvalid<AuthorizationServerId>();
        }
    });
}
