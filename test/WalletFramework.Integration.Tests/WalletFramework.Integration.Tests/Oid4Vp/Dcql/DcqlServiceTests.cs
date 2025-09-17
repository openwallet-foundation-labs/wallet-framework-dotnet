using FluentAssertions;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Moq;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Tests.Samples;
using WalletFramework.SdJwtLib.Roles.Implementation;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Integration.Tests.Oid4Vp.Dcql;

public class DcqlServiceTests
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

    private readonly SdJwtRecord _driverCredential = CreateCredential(JsonBasedCredentialSamples.DriverCredential, DriverLicenseCredentialSetId);
    private readonly SdJwtRecord _driverCredentialClone = CreateCredential(JsonBasedCredentialSamples.DriverCredential, DriverLicenseCredentialCloneSetId);
    private readonly SdJwtRecord _universityCredential = CreateCredential(JsonBasedCredentialSamples.UniversityCredential, UniversityCredentialSetId);
    private readonly SdJwtRecord _nestedCredential = CreateCredential(JsonBasedCredentialSamples.NestedCredential, NestedCredentialSetId);
    private readonly SdJwtRecord _alternativeNestedCredential = CreateCredential(JsonBasedCredentialSamples.AlternativeNestedCredential, AlternativeNestedCredentialSetId);
    private readonly SdJwtRecord _batchCredentialOne = CreateCredential(JsonBasedCredentialSamples.BatchCredential, BatchCredentialSetId);
    private readonly SdJwtRecord _batchCredentialTwo = CreateCredential(JsonBasedCredentialSamples.BatchCredential, BatchCredentialSetId);

    [Fact]
    public async Task Can_Get_Credential_Candidates_For_CredentialQuery()
    {
        // Arrange
        var idClaimQuery = CreateCredentialClaimQuery(["id"], "123");

        var batchCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [idClaimQuery, CreateCredentialClaimQuery(["issuer"]), CreateCredentialClaimQuery(["batchExp"])], 
            CreateCredentialMetaQuery([_batchCredentialOne.Vct]));

        var driverLicenseCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [idClaimQuery, CreateCredentialClaimQuery(["issuer"]), CreateCredentialClaimQuery(["dateOfBirth"])],
            CreateCredentialMetaQuery([_driverCredential.Vct]));

        var universityCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [CreateCredentialClaimQuery(["degree"])], 
            CreateCredentialMetaQuery([_universityCredential.Vct]));
        
        var driverLicenseCredentialSetCandidate = new CredentialSetCandidate(DriverLicenseCredentialSetId,
            new List<ICredential> { _driverCredential });
        var driverLicenseCredentialCloneSetCandidate = new CredentialSetCandidate(DriverLicenseCredentialCloneSetId,
            new List<ICredential> { _driverCredentialClone });
        var universityCredentialSetCandidate = new CredentialSetCandidate(UniversityCredentialSetId,
            new List<ICredential> { _universityCredential});
        var batchCredentialSetCandidate = new CredentialSetCandidate(BatchCredentialSetId,
            new List<ICredential> { _batchCredentialOne, _batchCredentialTwo });
        
        var expected = new List<PresentationCandidate>
        {
            new(driverLicenseCredentialQuery.Id, 
                new List<CredentialSetCandidate>
                {
                    driverLicenseCredentialSetCandidate,
                    driverLicenseCredentialCloneSetCandidate
                },
                driverLicenseCredentialQuery.Claims!.ToList()),
            new(universityCredentialQuery.Id, new List<CredentialSetCandidate> { universityCredentialSetCandidate }, universityCredentialQuery.Claims!.ToList()),
            new(batchCredentialQuery.Id, new List<CredentialSetCandidate> { batchCredentialSetCandidate }, batchCredentialQuery.Claims!.ToList()),
        };

        var dcqlService = CreateDcqlService();

        // Act
        var driverLicenseCandidate = (await dcqlService.QuerySingle(driverLicenseCredentialQuery)).UnwrapOrThrow();
        var universityCandidate = (await dcqlService.QuerySingle(universityCredentialQuery)).UnwrapOrThrow();
        var batchCandidate = (await dcqlService.QuerySingle(batchCredentialQuery)).UnwrapOrThrow();
        var credentialCandidatesArray = new List<PresentationCandidate> { driverLicenseCandidate, universityCandidate, batchCandidate };

        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
        
    [Fact]
    public async Task Can_Get_Credential_Candidates_For_CredentialQuery_With_Nested_Paths()
    {
        var alternativeNestedCredentialSetCandidate = new CredentialSetCandidate(AlternativeNestedCredentialSetId,
            new List<ICredential> { _alternativeNestedCredential });
    
        var cityClaimQuery = CreateCredentialClaimQuery(["address", "city"], "Berlin");
    
        var identityCredentialCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [cityClaimQuery, CreateCredentialClaimQuery(["vct"]), CreateCredentialClaimQuery(["iss"])], 
            CreateCredentialMetaQuery([_alternativeNestedCredential.Vct]));
    
        var expected = new List<PresentationCandidate>
        {
            new(identityCredentialCredentialQuery.Id,
                new List<CredentialSetCandidate> { alternativeNestedCredentialSetCandidate },
                identityCredentialCredentialQuery.Claims!.ToList())
        };
    
        var dcqlService = CreateDcqlService();
    
        // Act
        var candidate = 
            (await dcqlService.QuerySingle(identityCredentialCredentialQuery)).UnwrapOrThrow();
        var credentialCandidatesArray = new List<PresentationCandidate> { candidate };
    
        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Can_Get_Multiple_Credential_Candidates_For_CredentialQuery_With_Nested_Paths()
    {
        var nestedCredentialSetCandidate = new CredentialSetCandidate(NestedCredentialSetId,
            new List<ICredential> { _nestedCredential });
        var alternativeNestedCredentialSetCandidate = new CredentialSetCandidate(AlternativeNestedCredentialSetId,
            new List<ICredential> { _alternativeNestedCredential });
    
        var vctClaimQuery = CreateCredentialClaimQuery(["vct"], "IdentityCredential");
        
        var identityCredentialCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [vctClaimQuery, CreateCredentialClaimQuery(["address", "city"]), CreateCredentialClaimQuery(["iss"])], 
            CreateCredentialMetaQuery([_nestedCredential.Vct, _alternativeNestedCredential.Vct]));
    
        var expected = new List<PresentationCandidate>
        {
            new(identityCredentialCredentialQuery.Id,
                new List<CredentialSetCandidate> { nestedCredentialSetCandidate, alternativeNestedCredentialSetCandidate },
                identityCredentialCredentialQuery.Claims!.ToList()),
        };
    
        var dcqlService = CreateDcqlService();
    
        // Act
        var candidate = 
            (await dcqlService.QuerySingle(identityCredentialCredentialQuery)).UnwrapOrThrow();
        
        var credentialCandidatesArray = new List<PresentationCandidate> { candidate };
    
        // Assert
        credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_CredentialClaimQueries_Are_Fulfilled()
    {
        // Arrange
        var driverLicenseCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [
                CreateCredentialClaimQuery(["id"]),
                CreateCredentialClaimQuery(["iss"]),
                CreateCredentialClaimQuery(["dateOfBirth"]),
                CreateCredentialClaimQuery(["name"])
            ], 
            CreateCredentialMetaQuery([_driverCredential.Vct]));
    
        var dcqlService = CreateDcqlService();
    
        // Act
        var candidate = await dcqlService.QuerySingle(driverLicenseCredentialQuery);
    
        // Assert
        candidate.IsNone.Should().BeTrue();
    }
    
    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_CredentialClaimQueryValues_Are_Fulfilled()
    {
        // Arrange
        var idClaimQuery = CreateCredentialClaimQuery(["id"], "326");
    
        var driverLicenseCredentialQuery = CreateCredentialQuery(
            Guid.NewGuid().ToString(),
            Constants.SdJwtDcFormat,
            [idClaimQuery, CreateCredentialClaimQuery(["issuer"]), CreateCredentialClaimQuery(["dateOfBirth"])], 
            CreateCredentialMetaQuery([_driverCredential.Vct]));
    
        var dcqlService = CreateDcqlService();
    
        // Act
        var candidate = await dcqlService.QuerySingle(driverLicenseCredentialQuery);
    
        // Assert
        candidate.IsNone.Should().BeTrue();
    }

    private IDcqlService CreateDcqlService()
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
        
        return new DcqlService(_agentProviderMock.Object, _mdocStorageMock.Object, _sdJwtVcHolderServiceMock.Object);
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

    private static ClaimQuery CreateCredentialClaimQuery(JArray path, string? value = null)
    {
        var credentialQueryClaim = new ClaimQuery
        {
            Id = ClaimIdentifier.Validate(Guid.NewGuid().ToString()).UnwrapOrThrow(),
            Path = ClaimPath.FromJArray(path).UnwrapOrThrow()
        };

        if (value != null)
        {
            credentialQueryClaim.Values = [value];
        }
        
        return credentialQueryClaim;
    }
    
    private static CredentialMetaQuery CreateCredentialMetaQuery(string[] type)
    {
        return CredentialMetaQuery.FromJObject(new JObject()
        {
            ["vct_values"] = new JArray() { type }
        }).UnwrapOrThrow();
    }

    private static CredentialQuery CreateCredentialQuery(string id, string format, ClaimQuery[] credentialQueryClaims, CredentialMetaQuery metaQuery)
    {
        var credentialQuery = new CredentialQuery
        {
            Id = CredentialQueryId.Create(id).UnwrapOrThrow(),
            Format = format,
            Claims = credentialQueryClaims,
            Meta = metaQuery
        };
        return credentialQuery;
    }
}
