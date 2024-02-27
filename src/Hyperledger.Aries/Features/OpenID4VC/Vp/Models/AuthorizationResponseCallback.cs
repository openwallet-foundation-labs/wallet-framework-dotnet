using System;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    internal struct AuthorizationResponseCallback
    {
        [JsonProperty ("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
        internal Uri? RedirectUri { get; set; }
    }
}
