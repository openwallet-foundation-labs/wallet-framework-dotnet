#nullable enable

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    ///     Represents a request for an access token from an OAuth 2.0 Authorization Server.
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        ///     Gets or sets the grant type of the request. Determines the type of token request being made.
        /// </summary>
        public string GrantType { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the pre-authorized code. Represents the authorization to obtain specific credentials.
        ///     This is required if the grant type is urn:ietf:params:oauth:grant-type:pre-authorized_code.
        /// </summary>
        public string? PreAuthorizedCode { get; set; }
        
        /// <summary>
        ///     Gets or sets the pre-authorized code. Represents the authorization to obtain specific credentials.
        ///     This is required if the grant type is urn:ietf:params:oauth:grant-type:pre-authorized_code.
        /// </summary>
        public string? Code { get; set; }
        
        /// <summary>
        ///     Gets or sets the pre-authorized code. Represents the authorization to obtain specific credentials.
        ///     This is required if the grant type is urn:ietf:params:oauth:grant-type:pre-authorized_code.
        /// </summary>
        public string? CodeVerifier { get; set; }
        
        /// <summary>
        ///     Gets or sets the pre-authorized code. Represents the authorization to obtain specific credentials.
        ///     This is required if the grant type is urn:ietf:params:oauth:grant-type:pre-authorized_code.
        /// </summary>
        public string? ClientId { get; set; }
        
        /// <summary>
        ///     Gets or sets the pre-authorized code. Represents the authorization to obtain specific credentials.
        ///     This is required if the grant type is urn:ietf:params:oauth:grant-type:pre-authorized_code.
        /// </summary>
        public string? RedirectUri { get; set; }

        /// <summary>
        ///     Gets or sets the scope of the access request. Defines the permissions the client is asking for.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        ///     Gets or sets the transaction code. This value must be present if a transaction code was required in a previous step.
        /// </summary>
        public string? TransactionCode { get; set; }

        /// <summary>
        ///     Converts the properties of the TokenRequest instance into an FormUrlEncodedContent type suitable for HTTP POST
        ///     operations.
        /// </summary>
        /// <returns>Returns an instance of FormUrlEncodedContent containing the URL-encoded properties of the TokenRequest.</returns>
        public FormUrlEncodedContent ToFormUrlEncoded()
        {
            var keyValuePairs = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(GrantType))
                keyValuePairs.Add(new KeyValuePair<string, string>("grant_type", GrantType));

            if (!string.IsNullOrEmpty(PreAuthorizedCode))
                keyValuePairs.Add(new KeyValuePair<string, string>("pre-authorized_code", PreAuthorizedCode));

            if (!string.IsNullOrEmpty(Scope))
                keyValuePairs.Add(new KeyValuePair<string, string>("scope", Scope));

            if (!string.IsNullOrEmpty(TransactionCode))
                keyValuePairs.Add(new KeyValuePair<string, string>("tx_code", TransactionCode));
            
            if (!string.IsNullOrEmpty(Code))
                keyValuePairs.Add(new KeyValuePair<string, string>("code", Code));

            if (!string.IsNullOrEmpty(RedirectUri))
                keyValuePairs.Add(new KeyValuePair<string, string>("redirect_uri", RedirectUri));

            if (!string.IsNullOrEmpty(CodeVerifier))
                keyValuePairs.Add(new KeyValuePair<string, string>("code_verifier", CodeVerifier));
            
            if (!string.IsNullOrEmpty(ClientId))
                keyValuePairs.Add(new KeyValuePair<string, string>("client_id", ClientId));

            return new FormUrlEncodedContent(keyValuePairs);
        }
    }
}
