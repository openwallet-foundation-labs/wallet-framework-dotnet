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
    ///         Credentials: Contains issued Credentials. It MUST be present when transaction_id is not returned. It MAY be a
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
    public OneOf<List<Credential>, TransactionId> CredentialsOrTransactionId { get; }
    
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
    ///     OPTIONAL. The KeyId for the key which was used for the Key-Binding Proof. SD-JWTs may have no KeyId.
    /// </summary>
    public Option<KeyId> KeyId { get; }
    
    private CredentialResponse(
        OneOf<List<Credential>, TransactionId> credentialsOrTransactionId,
        Option<string> cNonce,
        Option<int> cNonceExpiresIn,
        Option<KeyId> keyId)
    {
        CNonceExpiresIn = cNonceExpiresIn;
        CredentialsOrTransactionId = credentialsOrTransactionId;
        CNonce = cNonce;
        KeyId = keyId;
    }
    
    private static CredentialResponse Create(
        OneOf<List<Credential>, TransactionId> credentialsOrTransactionId,
        Option<string> cNonce,
        Option<int> cNonceExpiresIn,
        Option<KeyId> keyId) =>
        new(credentialsOrTransactionId, cNonce, cNonceExpiresIn, keyId);

    public static Validation<CredentialResponse> ValidCredentialResponse(JObject response, Option<KeyId> keyId)
    {
        // TODO: Implement transactionID
        var singleCredential =
            from jToken in response.GetByKey("credential").ToOption()
            from jValue in jToken.ToJValue().ToOption()
            from cred in Credential.ValidCredential(jValue).ToOption()
            select cred;

        var batchCredentialsDraft14 =
            from jToken in response.GetByKey("credentials").ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from all in jArray.TraverseAll(jToken =>
                from jValue in jToken.ToJValue().ToOption()
                from cred in Credential.ValidCredential(jValue).ToOption()
                select cred)
            select all;
        
        var batchCredentialsDraft15 =
            from jToken in response.GetByKey("credentials").ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from all in jArray.TraverseAll(jToken =>
                from credToken in jToken.GetByKey("credential").ToOption()
                from jValue in credToken.ToJValue().ToOption()
                from cred in Credential.ValidCredential(jValue).ToOption()
                select cred)
            select all;
        
        var credentials = batchCredentialsDraft15.Match(
            Some: bcDraft15 => (OneOf<List<Credential>, TransactionId>)bcDraft15.ToList(),
            None: () => batchCredentialsDraft14.Match(
                Some: bcDraft14 => (OneOf<List<Credential>, TransactionId>)bcDraft14.ToList(),
                None: () => singleCredential.Match(
                    Some: c => (OneOf<List<Credential>, TransactionId>)new List<Credential> { c },
                    None: () => throw new InvalidOperationException("Credential response contains no credentials"))));

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
            .Apply(credentials)
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
