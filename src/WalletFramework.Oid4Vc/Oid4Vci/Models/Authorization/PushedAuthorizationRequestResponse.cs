using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    internal record PushedAuthorizationRequestResponse
    {
        [JsonProperty("request_uri")]
        public Uri RequestUri { get; init; }
        
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; init; }
        
        [JsonConstructor]
        private PushedAuthorizationRequestResponse(Uri requestUri, string expiresIn) 
            => (RequestUri, ExpiresIn) = (requestUri, expiresIn);
    }
}
