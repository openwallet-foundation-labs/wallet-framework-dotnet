using System;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    internal class AuthorizationResponseCallback
    {
        [JsonProperty ("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
        internal Uri? RedirectUri { get; set; }
    }
}
