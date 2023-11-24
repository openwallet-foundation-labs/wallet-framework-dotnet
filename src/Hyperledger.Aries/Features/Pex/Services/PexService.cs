using System;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Features.Pex.Models;

namespace Hyperledger.Aries.Features.Pex.Services
{
    /// <inheritdoc />
    public class PexService : IPexService
    {
        /// <inheritdoc />
        public Task<PresentationSubmission> CreatePresentationSubmission(PresentationDefinition presentationDefinition, DescriptorMap[] descriptorMaps)
        {
            var inputDescriptorIds = presentationDefinition.InputDescriptors.Select(x => x.Id);
            if (!descriptorMaps.Select(x => x.Id).All(inputDescriptorIds.Contains))
                throw new ArgumentException("Missing descriptors for given input descriptors in presentation definition.", nameof(descriptorMaps));
            
            var presentationSubmission = new PresentationSubmission
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = presentationDefinition.Id,
                DescriptorMap = descriptorMaps.Cast<DescriptorMap>().ToArray()
            };
            
            return Task.FromResult(presentationSubmission);
        }
    }
}
