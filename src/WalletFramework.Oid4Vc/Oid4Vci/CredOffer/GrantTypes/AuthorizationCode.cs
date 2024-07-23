using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes;

/// <summary>
///     Represents the parameters for the 'authorization_code' grant type.
/// </summary>
public record AuthorizationCode
{
    /// <summary>
    ///     String value created by the Credential Issuer and opaque to the Wallet that is used to bind the subsequent
    ///     Authorization Request with the Credential Issuer to a context set up during previous steps. If the Wallet decides
    ///     to use the Authorization Code Flow and received a value for this parameter, it MUST include it in the subsequent
    ///     Authorization Request to the Credential Issuer as the issuer_state parameter value
    /// </summary>
    [JsonProperty("issuer_state")]
    public Option<string> IssuerState { get; }

    /// <summary>
    ///     String that the Wallet can use to identify the Authorization Server to use with this grant type when
    ///     authorization_servers parameter in the Credential Issuer metadata has multiple entries. It MUST NOT be used
    ///     otherwise. The value of this parameter MUST match with one of the values in the authorization_servers array
    ///     obtained from the Credential Issuer metadata.
    /// </summary>
    [JsonProperty("authorization_server")]
    public Option<string> AuthorizationServer { get; }

    private AuthorizationCode(Option<string> issuerState, Option<string> authorizationServer)
    {
        IssuerState = issuerState;
        AuthorizationServer = authorizationServer;
    }

    public static Option<AuthorizationCode> OptionalAuthorizationCode(JToken authorizationCode)
    {
        var issuerState = authorizationCode
            .GetByKey("issuer_state")
            .OnSuccess(token => token.ToString())
            .ToOption();
        
        var authServer = authorizationCode
            .GetByKey("authorization_server")
            .OnSuccess(token => token.ToString())
            .ToOption();
        
        return new AuthorizationCode(issuerState, authServer);
    }
}
