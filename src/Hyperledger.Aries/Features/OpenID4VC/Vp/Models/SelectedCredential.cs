using Hyperledger.Aries.Storage.Models.Interfaces;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     Represents a credential that the Holder chose to present to the Verifier.
    /// </summary>
    public class SelectedCredential
    {
        /// <summary>
        ///     Gets or Sets the ID of the input descriptor that is answered.
        /// </summary>
        public string InputDescriptorId { get; set; }  = null!;
        
        /// <summary>
        ///     Gets or Sets the Credential that is used to answer the input descriptor.
        /// </summary>
        public ICredential Credential { get; set; } = null!;
    }
}
