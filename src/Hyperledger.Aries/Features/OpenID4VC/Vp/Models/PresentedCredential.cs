using System.Collections.Generic;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     Represents a presented credential.
    /// </summary>
    public class PresentedCredential
    {
        /// <summary>
        ///     Gets or Sets the claims of the credential that were presented.
        /// </summary>
        public Dictionary<string, PresentedClaim> PresentedClaims { get; set; } = null!;
        
        /// <summary>
        ///     Gets or Sets the credential id.
        /// </summary>
        public string CredentialId { get; set; } = null!;
    }
}
