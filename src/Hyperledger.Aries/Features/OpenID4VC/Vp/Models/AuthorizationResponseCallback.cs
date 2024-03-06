using System;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    internal record AuthorizationResponseCallback
    {
        [JsonProperty("redirect_uri")]
        private Uri? RedirectUri { get; }
        
        public static implicit operator Uri? (AuthorizationResponseCallback? response) => response?.RedirectUri;
        
        public static implicit operator AuthorizationResponseCallback (Uri redirectUri) => new (redirectUri);

        [JsonConstructor]
        private AuthorizationResponseCallback(Uri redirectUri)
        {
            RedirectUri = redirectUri;
        }
    }
}
