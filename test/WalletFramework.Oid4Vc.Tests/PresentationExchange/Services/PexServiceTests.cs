using FluentAssertions;
using Hyperledger.Aries.Storage.Models.Interfaces;
using Hyperledger.Aries.Tests.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Roles.Implementation;
using WalletFramework.Oid4Vc.Oid4Vp.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Tests.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.Tests.PresentationExchange.Services
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
        
        [Fact]
        public async Task Can_Get_Credential_Candidates_For_Input_Descriptors()
        {
            var driverLicenseCredential = CreateCredential(CredentialExamples.DriverCredential);
            var driverLicenseCredentialClone = CreateCredential(CredentialExamples.DriverCredential);
            var universityCredential = CreateCredential(CredentialExamples.UniversityCredential);

            var idFilter = new Filter();
            idFilter.PrivateSet(x => x.Type, "string");
            idFilter.PrivateSet(x => x.Const, "123");

            var driverLicenseInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[]
                    { CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.dateOfBirth") }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "EU Driver's License",
                "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.",
                new[] { "A" });

            var universityInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[] { CreateField("$.degree") }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "University Degree",
                "We can only accept digital university degrees.");

            var expected = new List<CredentialCandidates>
            {
                new CredentialCandidates(
                    driverLicenseInputDescriptor.Id,
                    new List<ICredential> { driverLicenseCredential, driverLicenseCredentialClone }),
                new CredentialCandidates(
                    universityInputDescriptor.Id, new List<ICredential> { universityCredential })
            };

            var sdJwtVcHolderService = CreatePexService();

            // Act
            var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
                new[]
                {
                    driverLicenseCredential, driverLicenseCredentialClone, universityCredential
                },
                new[] { driverLicenseInputDescriptor, universityInputDescriptor });

            // Assert
            credentialCandidatesArray.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task Can_Get_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
        {
            var driverLicenseCredential = CreateCredential(CredentialExamples.DriverCredential);
            var nestedCredential = CreateCredential(CredentialExamples.NestedCredential);
            var alternativeNestedCredential = CreateCredential(CredentialExamples.AlternativeNestedCredential);

            var idFilter = new Filter();
            idFilter.PrivateSet(x => x.Type, "string");
            idFilter.PrivateSet(x => x.Const, "Berlin");

            var identityCredentialInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[]
                    { CreateField("$.address.city", idFilter), CreateField("$.vct"), CreateField("$.iss") }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "Identity Credential",
                "We can only accept digital identity credentials.",
                new[] { "A" });

            var expected = new List<CredentialCandidates>
            {
                new CredentialCandidates(
                    identityCredentialInputDescriptor.Id,
                    new List<ICredential> { alternativeNestedCredential }),
            };

            var sdJwtVcHolderService = CreatePexService();

            // Act
            var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
                new[]
                {
                    driverLicenseCredential, alternativeNestedCredential, nestedCredential
                },
                new[] { identityCredentialInputDescriptor });

            // Assert
            credentialCandidatesArray.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task Can_Get_Multiple_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
        {
            var driverLicenseCredential = CreateCredential(CredentialExamples.DriverCredential);
            var nestedCredential = CreateCredential(CredentialExamples.NestedCredential);
            var alternativeNestedCredential = CreateCredential(CredentialExamples.AlternativeNestedCredential);

            var vctFilter = new Filter();
            vctFilter.PrivateSet(x => x.Const, "IdentityCredential");

            var identityCredentialInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[]
                    { CreateField("$.address.city"), CreateField("$.vct", vctFilter), CreateField("$.iss") }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "Identity Credential",
                "We can only accept digital identity credentials.",
                new[] { "A" });

            var expected = new List<CredentialCandidates>
            {
                new CredentialCandidates(
                    identityCredentialInputDescriptor.Id,
                    new List<ICredential> { nestedCredential, alternativeNestedCredential }),
            };

            var sdJwtVcHolderService = CreatePexService();

            // Act
            var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
                new[]
                {
                    driverLicenseCredential, alternativeNestedCredential, nestedCredential
                },
                new[] { identityCredentialInputDescriptor });

            // Assert
            credentialCandidatesArray.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Cant_Get_Credential_Candidates_When_Not_All_Fields_Are_Fulfilled()
        {
            // Arrange
            var driverLicenseClaims = new Dictionary<string, string>
            {
                { "id", "123" },
                { "issuer", "did:example:gov" },
                { "dateOfBirth", "01/01/2000" }
            };

            var employeeCredential = CreateCredential(CredentialExamples.UniversityCredential);

            var driverLicenseInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[]
                {
                    CreateField("$.id"), CreateField("$.issuer"),
                    CreateField("$.dateOfBirth"), CreateField("$.name")
                }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "EU Driver's License",
                "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");

            var sdJwtVcHolderService = CreatePexService();

            // Act
            Func<Task> act = async () =>  await sdJwtVcHolderService.FindCredentialCandidates(
                new[] { employeeCredential },
                new[] { driverLicenseInputDescriptor });

            // Assert
            await Assert.ThrowsAsync<Oid4VpNoCredentialCandidateException>(act);
        }

        [Fact]
        public async Task Cant_Get_Credential_Candidates_When_Not_All_Filters_Are_Fulfilled()
        {
            // Arrange
            var driverLicenseClaims = new Dictionary<string, string>
            {
                { "id", "123" },
                { "issuer", "did:example:gov" },
                { "dateOfBirth", "01/01/2000" }
            };

            var driverLicenseCredential = CreateCredential(CredentialExamples.DriverCredential);

            var idFilter = new Filter();
            idFilter.PrivateSet(x => x.Type, "string");
            idFilter.PrivateSet(x => x.Const, "326");

            var driverLicenseInputDescriptor = CreateInputDescriptor(
                CreateConstraints(new[]
                {
                    CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.dateOfBirth")
                }),
                new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
                Guid.NewGuid().ToString(),
                "EU Driver's License",
                "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");

            var sdJwtVcHolderService = CreatePexService();

            // Act
            Func<Task> act = async () => await sdJwtVcHolderService.FindCredentialCandidates(
                new[] { driverLicenseCredential },
                new[] { driverLicenseInputDescriptor });

            // Assert
            await Assert.ThrowsAsync<Oid4VpNoCredentialCandidateException>(act);
        }

        #region Helper Methods
        private static IPexService CreatePexService()
        {
            return new PexService();
        }
        
        private static SdJwtRecord CreateCredential(JObject payload)
        {
            const string jwk = "{\"kty\":\"EC\",\"d\":\"1_2Dagk1gvTIOX-DLPe7GHNsGLJMc7biySNA-so7TXE\",\"use\":\"sig\",\"crv\":\"P-256\",\"x\":\"X6sZhH_kFp_oKYiPXW-LvUyAv9mHp1xYcpAK3yy0wGY\",\"y\":\"p0URU7tgWbh42miznti0NVKM36fpJBbIfnF8ZCYGryE\",\"alg\":\"ES256\"}";

            var issuedSdJwt = new Issuer().IssueCredential(payload, jwk);
            
            var record = new SdJwtRecord(issuedSdJwt.IssuanceFormat, new Dictionary<string, ClaimMetadata>(),
                new List<CredentialDisplayMetadata>(), new Dictionary<string, string>(), "0");

            return record;
        }
        
        private static Constraints CreateConstraints(Field[] fields)
        {
            var constraints = new Constraints();
            constraints.PrivateSet(x => x.Fields, fields);

            return constraints;
        }

        private static Field CreateField(string path, Filter? filter = null)
        {
            var field = new Field();
            field.PrivateSet(x => x.Path, new[] { path });

            if (filter != null)
            {
                field.PrivateSet(x => x.Filter, filter);
            }

            return field;
        }

        private static Format CreateFormat(string[] supportedAlg)
        {
            var format = new Format();
            format.PrivateSet(x => x.Alg, supportedAlg);

            return format;
        }

        private static InputDescriptor CreateInputDescriptor(Constraints constraints, Dictionary<string, Format> formats, string id,
            string name, string purpose, string[]? group = null)
        {
            var inputDescriptor = new InputDescriptor();

            inputDescriptor.PrivateSet(x => x.Constraints, constraints);
            inputDescriptor.PrivateSet(x => x.Formats, formats);
            inputDescriptor.PrivateSet(x => x.Id, id);
            inputDescriptor.PrivateSet(x => x.Name, name);
            inputDescriptor.PrivateSet(x => x.Purpose, purpose);

            if (group != null)
            {
                inputDescriptor.PrivateSet(x => x.Group, group);
            }

            return inputDescriptor;
        }
        #endregion
    }
}
