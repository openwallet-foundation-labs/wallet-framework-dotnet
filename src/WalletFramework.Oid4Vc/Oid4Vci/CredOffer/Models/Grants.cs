using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes.AuthorizationCode;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes.PreAuthorizedCode;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

/// <summary>
///     Represents the grant types that the Credential Issuer's AS is prepared to process for the credential offer.
/// </summary>
public record Grants
{
    /// <summary>
    ///     Gets the authorization_code grant type parameters. This includes an optional issuer state that is used to
    ///     bind the subsequent Authorization Request with the Credential Issuer to a context set up during previous steps.
    /// </summary>
    [JsonProperty("authorization_code")]
    public Option<AuthorizationCode> AuthorizationCode { get; }

    /// <summary>
    ///     Gets the pre-authorized_code grant type parameters. This includes a required pre-authorized code
    ///     representing the Credential Issuer's authorization for the Wallet to obtain Credentials of a certain type, and an
    ///     optional boolean specifying whether a user PIN is required along with the Token Request.
    /// </summary>
    [JsonProperty("urn:ietf:params:oauth:grant-type:pre-authorized_code")]
    public Option<PreAuthorizedCode> PreAuthorizedCode { get; }

    private Grants(Option<AuthorizationCode> authorizationCode, Option<PreAuthorizedCode> preAuthorizedCode)
    {
        AuthorizationCode = authorizationCode;
        PreAuthorizedCode = preAuthorizedCode;
    }

    public static Option<Grants> OptionalGrants(JToken grants)
    {
        var authorizationCode = grants
            .GetByKey("authorization_code")
            .ToOption()
            .OnSome(OptionalAuthorizationCode);

        var preAuthorizedCode = grants
            .GetByKey("urn:ietf:params:oauth:grant-type:pre-authorized_code")
            .ToOption()
            .OnSome(OptionalPreAuthorizedCode);

        if (authorizationCode.IsNone && preAuthorizedCode.IsNone)
            return Option<Grants>.None;
        
        return new Grants(authorizationCode, preAuthorizedCode);
    }
}
