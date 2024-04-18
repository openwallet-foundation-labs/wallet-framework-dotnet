using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services
{
    /// <summary>
    /// Pex Service.
    /// </summary>
    public interface IPexService
    {
        /// <summary>
        /// Creates a presentation submission.
        /// </summary>
        /// <param name="presentationDefinition">The presentation definition.</param>
        /// <param name="descriptorMaps">Data used to build Descriptor Maps.</param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains the Presentation Submission.
        /// </returns>
        Task<PresentationSubmission> CreatePresentationSubmission(PresentationDefinition presentationDefinition, DescriptorMap[] descriptorMaps);
    }
}
