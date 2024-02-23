using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.Pex.Services;
using Hyperledger.Aries.Tests.Extensions;
using Hyperledger.Aries.Tests.Features.Pex.Models;
using Newtonsoft.Json;
using Xunit;

namespace Hyperledger.Aries.Tests.Features.Pex.Services
{
    public class PexServiceTests
    {
        private readonly PexService _pexService = new PexService();

        [Fact]
        public async Task Can_Create_Presentation_Submission()
        {
            var presentationDefinition = JsonConvert.DeserializeObject<PresentationDefinition>(PexTestsDataProvider.GetJsonForTestCase());
            
            var credentials = new[]
            {
                new DescriptorMap
                {
                    Id = presentationDefinition.InputDescriptors[0].Id,
                    Format = presentationDefinition.InputDescriptors[0].Formats.First().Key,
                    Path = "$.credentials[0]"
                },
                new DescriptorMap
                {
                    Id = presentationDefinition.InputDescriptors[1].Id,
                    Format = presentationDefinition.InputDescriptors[1].Formats.First().Key,
                    Path = "$.credentials[1]"
                },
            };

            
            var presentationSubmission = await _pexService.CreatePresentationSubmission(presentationDefinition, credentials);

            presentationSubmission.Id.Should().NotBeNullOrWhiteSpace();
            presentationSubmission.DefinitionId.Should().Be(presentationDefinition.Id);
            presentationSubmission.DescriptorMap.Length.Should().Be(credentials.Length);

            for (var i = 0; i < presentationDefinition.InputDescriptors.Length; i++)
            {
                presentationSubmission.DescriptorMap[i].Id.Should().Be(presentationDefinition.InputDescriptors[i].Id);
                presentationSubmission.DescriptorMap[i].Format.Should().Be(credentials[i].Format);
                presentationSubmission.DescriptorMap[i].Path.Should().Be(credentials[i].Path);   
            }
        }
    }
}
