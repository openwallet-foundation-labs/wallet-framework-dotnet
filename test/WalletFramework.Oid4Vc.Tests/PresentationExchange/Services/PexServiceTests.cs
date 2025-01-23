using FluentAssertions;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Roles.Implementation;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
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

    private static readonly CredentialSetId DriverLicenseCredentialSetId = CredentialSetId.CreateCredentialSetId();
    private static readonly CredentialSetId DriverLicenseCredentialCloneSetId = CredentialSetId.CreateCredentialSetId();
    private static readonly CredentialSetId UniversityCredentialSetId = CredentialSetId.CreateCredentialSetId();
    private static readonly CredentialSetId NestedCredentialSetId = CredentialSetId.CreateCredentialSetId();
    private static readonly CredentialSetId AlternativeNestedCredentialSetId = CredentialSetId.CreateCredentialSetId();
    private static readonly CredentialSetId BatchCredentialSetId = CredentialSetId.CreateCredentialSetId();

    private readonly SdJwtRecord _driverCredential = CreateCredential(CredentialExamples.DriverCredential, DriverLicenseCredentialSetId);
    private readonly SdJwtRecord _driverCredentialClone = CreateCredential(CredentialExamples.DriverCredential, DriverLicenseCredentialCloneSetId);
    private readonly SdJwtRecord _universityCredential = CreateCredential(CredentialExamples.UniversityCredential, UniversityCredentialSetId);
    private readonly SdJwtRecord _nestedCredential = CreateCredential(CredentialExamples.NestedCredential, NestedCredentialSetId);
    private readonly SdJwtRecord _alternativeNestedCredential = CreateCredential(CredentialExamples.AlternativeNestedCredential, AlternativeNestedCredentialSetId);
    private readonly SdJwtRecord _batchCredentialOne = CreateCredential(CredentialExamples.BatchCredential, BatchCredentialSetId);
    private readonly SdJwtRecord _batchCredentialTwo = CreateCredential(CredentialExamples.BatchCredential, BatchCredentialSetId);

    [Fact]
    public async Task Can_Create_Presentation_Submission()
    {
        var presentationDefinition = JsonConvert.DeserializeObject<PresentationDefinition>(PexTestsDataProvider.GetJsonForTestCase());
            
        var credentials = new[]
        {
            new DescriptorMap
            {
                Id = presentationDefinition.InputDescriptors[0].Id,
                Format = presentationDefinition.InputDescriptors[0].Formats.SdJwtFormat.IssuerSignedJwtAlgValues.First(),
                Path = "$.credentials[0]"
            },
            new DescriptorMap
            {
                Id = presentationDefinition.InputDescriptors[1].Id,
                Format = presentationDefinition.InputDescriptors[1].Formats.SdJwtFormat.IssuerSignedJwtAlgValues.First(),
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
        var idFilter = new Filter();
        idFilter.PrivateSet(x => x.Type, "string");
        idFilter.PrivateSet(x => x.Const, "123");

        var batchInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[]
                { CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.batchExp") }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "EU Driver's License",
            "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.",
            new[] { "A" });
        
        var driverLicenseInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[]
                { CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.dateOfBirth") }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "EU Driver's License",
            "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.",
            new[] { "A" });

        var universityInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[] { CreateField("$.degree") }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "University Degree",
            "We can only accept digital university degrees.");
        
        var driverLicenseCredentialSetCandidate = new CredentialSetCandidate(DriverLicenseCredentialSetId,
            new List<ICredential> { _driverCredential });
        var driverLicenseCredentialCloneSetCandidate = new CredentialSetCandidate(DriverLicenseCredentialCloneSetId,
            new List<ICredential> { _driverCredentialClone });
        var universityCredentialSetCandidate = new CredentialSetCandidate(UniversityCredentialSetId,
            new List<ICredential> { _universityCredential});
        var batchCredentialSetCandidate = new CredentialSetCandidate(BatchCredentialSetId,
            new List<ICredential> { _batchCredentialOne });
        
        var expected = new List<PresentationCandidates>
        {
            new(driverLicenseInputDescriptor.Id, 
                new List<CredentialSetCandidate>
                {
                    driverLicenseCredentialSetCandidate,
                    driverLicenseCredentialCloneSetCandidate
                }),
            new(universityInputDescriptor.Id, new List<CredentialSetCandidate> { universityCredentialSetCandidate }),
            new(batchInputDescriptor.Id, new List<CredentialSetCandidate> { batchCredentialSetCandidate }),
        };

        var pexService = CreatePexService();

        // Act
        var credentialCandidatesArray = await pexService.FindCredentialCandidates(
            [driverLicenseInputDescriptor, universityInputDescriptor, batchInputDescriptor],
            Option<Formats>.None);

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
        
    [Fact]
    public async Task Can_Get_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
    {
        var alternativeNestedCredentialSetCandidate = new CredentialSetCandidate(AlternativeNestedCredentialSetId,
            new List<ICredential> { _alternativeNestedCredential });

        var idFilter = new Filter();
        idFilter.PrivateSet(x => x.Type, "string");
        idFilter.PrivateSet(x => x.Const, "Berlin");

        var identityCredentialInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[]
                { CreateField("$.address.city", idFilter), CreateField("$.vct"), CreateField("$.iss") }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "Identity Credential",
            "We can only accept digital identity credentials.",
            new[] { "A" });

        var expected = new List<PresentationCandidates>
        {
            new(identityCredentialInputDescriptor.Id,
                new List<CredentialSetCandidate> { alternativeNestedCredentialSetCandidate })
        };

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates([identityCredentialInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
        
    [Fact]
    public async Task Can_Get_Multiple_Credential_Candidates_For_Input_Descriptors_With_Nested_Paths()
    {
        var nestedCredentialSetCandidate = new CredentialSetCandidate(NestedCredentialSetId,
            new List<ICredential> { _nestedCredential });
        var alternativeNestedCredentialSetCandidate = new CredentialSetCandidate(AlternativeNestedCredentialSetId,
            new List<ICredential> { _alternativeNestedCredential });

        var vctFilter = new Filter();
        vctFilter.PrivateSet(x => x.Const, "IdentityCredential");

        var identityCredentialInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[]
                { CreateField("$.address.city"), CreateField("$.vct", vctFilter), CreateField("$.iss") }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "Identity Credential",
            "We can only accept digital identity credentials.",
            new[] { "A" });

        var expected = new List<PresentationCandidates>
        {
            new(identityCredentialInputDescriptor.Id,
                new List<CredentialSetCandidate> { nestedCredentialSetCandidate, alternativeNestedCredentialSetCandidate }),
        };

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates([identityCredentialInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Can_Get_Credential_Candidates_For_Input_Descriptors_With_Enum_Filter()
    {
        var credentialSetCandidate = new CredentialSetCandidate(UniversityCredentialSetId,
            new List<ICredential> { _universityCredential });

        var enumVctFilter = new Filter();
        enumVctFilter.PrivateSet(x => x.Enum, new[] { "UniversityDegreeCredential", "vctTypeTwo" });
        enumVctFilter.PrivateSet(x => x.Type, "string");

        var universityInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[] { CreateField("$.degree"), CreateField("$.vct", enumVctFilter) }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "University Degree",
            "We can only accept digital university degrees.");

        var expected = new List<PresentationCandidates>
        {
            new(universityInputDescriptor.Id,
                new List<CredentialSetCandidate> { credentialSetCandidate }),
        };

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates([universityInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Cant_Get_Credential_Candidates_For_Input_Descriptors_With_Enum_Filter_Not_Fulfilled()
    {
        var enumVctFilter = new Filter();
        enumVctFilter.PrivateSet(x => x.Enum, new[] { "vctTypeTwo", "vctTypeTwo" });
        enumVctFilter.PrivateSet(x => x.Type, "string");

        var universityInputDescriptor = CreateInputDescriptor(
            CreateConstraints(new[] { CreateField("$.degree"), CreateField("$.vct", enumVctFilter) }),
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "University Degree",
            "We can only accept digital university degrees.");
        
        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates([universityInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidatesArray.Should().BeEmpty();
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
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "EU Driver's License",
            "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidates = await sdJwtVcHolderService.FindCredentialCandidates([driverLicenseInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidates.Should().BeEmpty();
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
            CreateFormat(new[] { "ES256" }),
            Guid.NewGuid().ToString(),
            "EU Driver's License",
            "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");

        var sdJwtVcHolderService = CreatePexService();

        // Act
        var credentialCandidates = await sdJwtVcHolderService.FindCredentialCandidates([driverLicenseInputDescriptor], Option<Formats>.None);

        // Assert
        credentialCandidates.Should().BeEmpty();
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
                _alternativeNestedCredential,
                _batchCredentialOne,
                _batchCredentialTwo
            });
        
        return new PexService(_agentProviderMock.Object, _mdocStorageMock.Object, _sdJwtVcHolderServiceMock.Object);
    }
        
    private static SdJwtRecord CreateCredential(JObject payload, CredentialSetId credentialSetId)
    {
        // Arrange
        const string jwk = "{\"kty\":\"EC\",\"d\":\"1_2Dagk1gvTIOX-DLPe7GHNsGLJMc7biySNA-so7TXE\",\"use\":\"sig\",\"crv\":\"P-256\",\"x\":\"X6sZhH_kFp_oKYiPXW-LvUyAv9mHp1xYcpAK3yy0wGY\",\"y\":\"p0URU7tgWbh42miznti0NVKM36fpJBbIfnF8ZCYGryE\",\"alg\":\"ES256\"}";
        var issuedSdJwt = new Issuer().IssueCredential(payload, jwk);
        var keyId = KeyId.CreateKeyId();

        var record = new SdJwtRecord(
            issuedSdJwt.IssuanceFormat,
            new Dictionary<string, ClaimMetadata>(),
            new List<SdJwtDisplay>(), 
            keyId,
            credentialSetId);
            
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

    private static Formats CreateFormat(string[] supportedAlg)
    {
        var format = new Formats()
        {
            SdJwtFormat = new SdJwtFormat()
            {
                IssuerSignedJwtAlgValues = supportedAlg.ToList()
            }
        };
        
        return format;
    }

    private static InputDescriptor CreateInputDescriptor(Constraints constraints, Formats formats, string id,
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
