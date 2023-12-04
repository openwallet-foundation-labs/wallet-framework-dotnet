using Hyperledger.Aries.Storage;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///    Used to persist OpenId4Vp presentations. 
    /// </summary>
    public class OidPresentationRecord : RecordBase
    {
        /// <summary>
        ///     Gets or sets the client id and identifies the Verifier.
        /// </summary>
        public string ClientId { get; set; } = null!;
        
        /// <summary>
        ///     Gets or sets the metadata of the Verifier.
        /// </summary>
        public string? ClientMetadata { get; set; }
        
        /// <summary>
        ///     Gets or sets the credentials the Holder presented to the Verifier.
        /// </summary>
        public PresentedCredential[] PresentedCredentials { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the type name.
        /// </summary>
        public override string TypeName => "AF.OidPresentationRecord";
    }
}
