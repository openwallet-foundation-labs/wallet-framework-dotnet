using FluentAssertions;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Roles.Implementation;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Tests.Extensions;
using WalletFramework.Oid4Vc.Tests.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Tests.PresentationExchange.Services;

public class PexServiceTests
{
    private readonly Mock<IAgentProvider> _agentProviderMock = new();
    private readonly Mock<IMdocStorage> _mdocStorageMock = new();
    private readonly Mock<ISdJwtVcHolderService> _sdJwtVcHolderServiceMock = new();

    private readonly SdJwtRecord _driverCredential = CreateCredential(CredentialExamples.DriverCredential);
    private readonly SdJwtRecord _driverCredentialClone = CreateCredential(CredentialExamples.DriverCredential);
    private readonly SdJwtRecord _universityCredential = CreateCredential(CredentialExamples.UniversityCredential);
    private readonly SdJwtRecord _nestedCredential = CreateCredential(CredentialExamples.NestedCredential);
    private readonly SdJwtRecord _alternativeNestedCredential = CreateCredential(CredentialExamples.AlternativeNestedCredential);

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
            
        var presentationSubmission = await CreatePexService().CreatePresentationSubmission(presentationDefinition, credentials);

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
        // Arrange
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
            new(driverLicenseInputDescriptor.Id, 
                new List<ICredential>
                {
                    driverLicenseCredential,
                    driverLicenseCredentialClone
                }),
            new(universityInputDescriptor.Id, new List<ICredential> { universityCredential })
        };

        var pexService = CreatePexService();

        // Act
        var credentialCandidatesArray = await pexService.FindCredentialCandidates(
            new[] { driverLicenseInputDescriptor, universityInputDescriptor });

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
        
    [Fact]
    public async Task Can_Get_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
    {
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
            new(identityCredentialInputDescriptor.Id,
                new List<ICredential> { alternativeNestedCredential })
        };

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
            new[] { identityCredentialInputDescriptor });

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
        
    [Fact]
    public async Task Can_Get_Multiple_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
    {
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
            new(identityCredentialInputDescriptor.Id,
                new List<ICredential> { nestedCredential, alternativeNestedCredential }),
        };

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
            new[] { identityCredentialInputDescriptor });

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_Fields_Are_Fulfilled()
    {
        // Arrange
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
            new[] { driverLicenseInputDescriptor });

        // Assert
        await Assert.ThrowsAsync<Oid4VpNoCredentialCandidateException>(act);
    }

    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_Filters_Are_Fulfilled()
    {
        // Arrange
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
            new[] { driverLicenseInputDescriptor }
        );

        // Assert
        await Assert.ThrowsAsync<Oid4VpNoCredentialCandidateException>(act);
    }

    #region Helper Methods
    private IPexService CreatePexService()
    {
        _sdJwtVcHolderServiceMock
            .Setup(x => x.ListAsync(It.IsAny<IAgentContext>(), It.IsAny<ISearchQuery?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(() => new List<SdJwtRecord>
            { 
                _driverCredential,
                _driverCredentialClone,
                _universityCredential,
                _nestedCredential,
                _alternativeNestedCredential 
            });
        
        return new PexService(_agentProviderMock.Object, _mdocStorageMock.Object, _sdJwtVcHolderServiceMock.Object);
    }
        
    private static SdJwtRecord CreateCredential(JObject payload)
    {
        // Arrange
        const string jwk = "{\"kty\":\"EC\",\"d\":\"1_2Dagk1gvTIOX-DLPe7GHNsGLJMc7biySNA-so7TXE\",\"use\":\"sig\",\"crv\":\"P-256\",\"x\":\"X6sZhH_kFp_oKYiPXW-LvUyAv9mHp1xYcpAK3yy0wGY\",\"y\":\"p0URU7tgWbh42miznti0NVKM36fpJBbIfnF8ZCYGryE\",\"alg\":\"ES256\"}";
        var issuedSdJwt = new Issuer().IssueCredential(payload, jwk);
        var keyId = KeyId.CreateKeyId();

        var record = new SdJwtRecord(
            issuedSdJwt.IssuanceFormat,
            new Dictionary<string, ClaimMetadata>(),
            new List<SdJwtDisplay>(), 
            keyId);
            
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
