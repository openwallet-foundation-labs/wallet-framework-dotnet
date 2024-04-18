using System.IdentityModel.Tokens.Jwt;
using System.Web;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models
{
    /// <summary>
    ///     Represents the Request of a Verifier to a Holder within the OpenId4VP specification.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        ///     Gets or sets the response type. Determines what the Authorization Response should contain.
        /// </summary>
        [JsonProperty("response_type")] 
        public string ResponseType { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the client id. The Identifier of the Verifier
        /// </summary>
        [JsonProperty("client_id")] 
        public string ClientId { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the client id scheme.
        /// </summary>
        [JsonProperty("client_id_scheme")] 
        public string? ClientIdScheme { get; set; }

        /// <summary>
        ///    Gets or sets the redirect uri.
        /// </summary>
        [JsonProperty("redirect_uri")] 
        public string? RedirectUri { get; set; }
        
        /// <summary>
        ///     Gets or sets the client metadata. Contains the Verifier metadata
        /// </summary>
        [JsonProperty("client_metadata")] 
        public string? ClientMetadata { get; set; }

        /// <summary>
        ///    Gets or sets the client metadata uri. Can be used to retrieve the verifier metadata.
        /// </summary>
        [JsonProperty("client_metadata_uri")] 
        public string? ClientMetadataUri { get; set; }

        /// <summary>
        ///    The scope of the request.
        /// </summary>
        [JsonProperty("scope")] 
        public string? Scope { get; set; }

        /// <summary>
        ///     Gets or sets the nonce. Random string for session binding.
        /// </summary>
        [JsonProperty("nonce")] 
        public string Nonce { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the response mode. Determines how to send the Authorization Response.
        /// </summary>
        [JsonProperty("response_mode")] 
        public string? ResponseMode { get; set; }

        /// <summary>
        ///  Gets or sets the response mode. Determines where to send the Authorization Response to.
        /// </summary>
        [JsonProperty("response_uri")]
        public string ResponseUri { get; set; } = null!;

        /// <summary>
        ///    Gets or sets the state.
        /// </summary>
        [JsonProperty("state")] 
        public string? State { get; set; }

        /// <summary>
        ///    Gets or sets the presentation definition. Contains the claims that the Verifier wants to receive.
        /// </summary>
        [JsonIgnore]
        public PresentationDefinition PresentationDefinition { get; set; } = null!;

        /// <summary>
        ///  Parses an Authorization Request from a Jwt.
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns>The AuthorizationRequest</returns>
        public static AuthorizationRequest? ParseFromJwt(string jwt)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadToken(jwt) as JwtSecurityToken;
            if (token == null) return null;
            
            var presentationDefinition = JsonConvert.DeserializeObject<PresentationDefinition>(token.Payload["presentation_definition"].ToString());
            var authorizationRequest = JsonConvert.DeserializeObject<AuthorizationRequest>(token.Payload.SerializeToJson());
            
            if (!(authorizationRequest != null && presentationDefinition != null)) 
                return null;

            authorizationRequest.PresentationDefinition = presentationDefinition;
            
            return authorizationRequest;
        }

        /// <summary>
        ///    Parses an Authorization Request from a Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>The AuthorizationRequest</returns>
        public static AuthorizationRequest? ParseFromUri(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            var dict = query.AllKeys.ToDictionary(key => key, key => query[key]);
            var json = JsonConvert.SerializeObject(dict);

            var presentationDefinition = JsonConvert.DeserializeObject<PresentationDefinition>(query["presentation_definition"]);
            var authorizationRequest = JsonConvert.DeserializeObject<AuthorizationRequest>(json);
            
            if (!(authorizationRequest != null && presentationDefinition != null)) 
                return null;
            
            authorizationRequest.PresentationDefinition = presentationDefinition;
            
            return authorizationRequest;
        }
    }

    /// <summary>
    ///    Extension methods for the <see cref="AuthorizationRequest"/> class.
    /// </summary>
    public static class AuthorizationRequestExtension
    {
        /// <summary>
        ///   Checks if the Authorization Request is HAIP conform.
        /// </summary>
        /// <param name="authorizationRequest"></param>
        /// <returns>Returns bool indicating whether the AuthorizationRequest is haip conform</returns>
        public static bool IsHaipConform(this AuthorizationRequest authorizationRequest)
        {
            if (authorizationRequest.ResponseType != "vp_token")
                return false;
            
            if (authorizationRequest.ResponseMode != "direct_post")
                return false;
            
            if (String.IsNullOrEmpty(authorizationRequest.ResponseUri))
                return false;
            
            if (!String.IsNullOrEmpty(authorizationRequest.RedirectUri))
                return false;

            if (authorizationRequest.ClientIdScheme == "redirect_uri"
                & authorizationRequest.ClientId != authorizationRequest.ResponseUri)
                return false;
            
            //TODO: Not supported yet
            //if (!(authorizationRequest.ClientIdScheme == "x509_san_dns" || authorizationRequest.ClientIdScheme == "verifier_attestation")) 
            //return false;

            return true;
        }
    }
}
