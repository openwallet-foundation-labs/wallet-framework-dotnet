using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.CredResponse.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.CredResponse.SdJwt;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredResponse;

/// <summary>
///     Represents a Credential Response. The response can be either immediate or deferred. In the synchronous response,
///     the issued Credential is immediately returned to the client. In the deferred response, a transaction ID
///     is sent to the client, which will be used later to retrieve the Credential once it's ready.
/// </summary>
public record CredentialResponse
{
    /// <summary>
    ///     <para>
    ///         Credential: Contains issued Credential. It MUST be present when transaction_id is not returned. It MAY be a
    ///         string or an object, depending on the Credential format
    ///     </para>
    ///     <para>
    ///         TransactionId: String identifying a Deferred Issuance transaction. This claim is contained in the response if
    ///         the Credential Issuer was unable to immediately issue the Credential. The value is subsequently used to obtain the
    ///         respective Credential with the Deferred Credential Endpoint. It MUST be present when the
    ///         credential parameter is not returned. It MUST be invalidated after the Credential for which it was meant
    ///         has been obtained by the Wallet
    ///     </para>
    /// </summary>
    public OneOf<Credential, TransactionId> CredentialOrTransactionId { get; }
    
    /// <summary>
    ///     OPTIONAL. JSON string containing a nonce to be used to create a proof of possession of key material
    ///     when requesting a Credential
    /// </summary>
    public Option<string> CNonce { get; }
    
    /// <summary>
    ///     OPTIONAL. JSON integer denoting the lifetime in seconds of the c_nonce
    /// </summary>
    public Option<int> CNonceExpiresIn { get; }

    /// <summary>
    ///     The KeyId for the key which was used for the Key-Binding Proof
    /// </summary>
    public KeyId KeyId { get; }
    
    private CredentialResponse(
        OneOf<Credential, TransactionId> credentialOrTransactionId,
        Option<string> cNonce,
        Option<int> cNonceExpiresIn,
        KeyId keyId)
    {
        CNonceExpiresIn = cNonceExpiresIn;
        CredentialOrTransactionId = credentialOrTransactionId;
        CNonce = cNonce;
        KeyId = keyId;
    }
    
    private static CredentialResponse Create(
        OneOf<Credential, TransactionId> credentialOrTransactionId,
        Option<string> cNonce,
        Option<int> cNonceExpiresIn,
        KeyId keyId) =>
        new(credentialOrTransactionId, cNonce, cNonceExpiresIn, keyId);

    public static Validation<CredentialResponse> ValidCredentialResponse(JObject response, KeyId keyId)
    {
        // TODO: Implement transactionID
        var credential =
            from jToken in response.GetByKey("credential")
            from jValue in jToken.ToJValue()
            from cred in Credential.ValidCredential(jValue)
            select (OneOf<Credential, TransactionId>)cred;

        var cNonce = response
            .GetByKey("c_nonce")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<CredentialResponse>();
                }

                return ValidationFun.Valid(str);
            })
            .ToOption();
        
        var cNonceExpiresIn = response
            .GetByKey("c_nonce_expires_in")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (int.TryParse(str, out var result))
                {
                    return ValidationFun.Valid(result);
                }

                return new JValueIsNotAnIntError(str);
            })
            .ToOption();
        
        return ValidationFun.Valid(Create)
            .Apply(credential)
            .Apply(cNonce)
            .Apply(cNonceExpiresIn)
            .Apply(keyId);
    }
    
    public readonly struct Credential
    {
        public OneOf<EncodedSdJwt, EncodedMdoc> Value { get; }
        
        private Credential(OneOf<EncodedSdJwt, EncodedMdoc> value)
        {
            Value = value;
        }

        public static Validation<Credential> ValidCredential(JValue credential)
        {
            var firstValid = new List<Validator<JValue, Credential>>
            {
                value => EncodedSdJwt.ValidEncodedSdJwt(value).OnSuccess(sdJwt => new Credential(sdJwt)),
                value => EncodedMdoc.ValidEncodedMdoc(value).OnSuccess(mdoc => new Credential(mdoc))
            }
            .FirstValid();

            return firstValid(credential);
        }
    }
}
