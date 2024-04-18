using Moq;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.Oid4Vc.Tests.Helpers;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Services;

public class Oid4VpClientServiceTests
{
    // Todo: Implement tests
    [Fact]
    public async Task Can_Get_Credential_Candidates_For_Input_Descriptors()
    {
        // // Arrange
        // var driverLicenseClaims = new Dictionary<string, string>
        // {
        //     { "id", "123" },
        //     { "issuer", "did:example:gov" },
        //     { "dateOfBirth", "01/01/2000" }
        // };
        //
        // var universityCredentialClaims = new Dictionary<string, string>
        // {
        //     { "degree", "Master of Science" },
        //     { "universityName", "ExampleUniversity" }
        // };
        //
        // var driverLicenseCredential = CreateCredential(driverLicenseClaims);
        // var driverLicenseCredentialClone = CreateCredential(driverLicenseClaims);
        // var universityCredential = CreateCredential(universityCredentialClaims);
        //
        // var idFilter = new Filter();
        // idFilter.PrivateSet(x => x.Type, "string");
        // idFilter.PrivateSet(x => x.Const, "123");
        //
        // var driverLicenseInputDescriptor = CreateInputDescriptor(
        //     CreateConstraints(new[]
        //         { CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.dateOfBirth") }),
        //     new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
        //     Guid.NewGuid().ToString(),
        //     "EU Driver's License",
        //     "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.",
        //     new[] { "A" });
        //
        // var universityInputDescriptor = CreateInputDescriptor(
        //     CreateConstraints(new[] { CreateField("$.degree") }),
        //     new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
        //     Guid.NewGuid().ToString(),
        //     "University Degree",
        //     "We can only accept digital university degrees.");
        //
        // var expected = new List<CredentialCandidates>
        // {
        //     new CredentialCandidates(
        //         driverLicenseInputDescriptor.Id,
        //         new List<ICredential> { driverLicenseCredential, driverLicenseCredentialClone }),
        //     new CredentialCandidates(
        //         universityInputDescriptor.Id, new List<ICredential> { universityCredential })
        // };
        //
        // var oid4vpClientService = CreateOid4VpClientService();
        //
        // // Act
        // var credentialCandidatesArray = await oid4vpClientService.PrepareAndSendAuthorizationResponseAsync(
        //     new[]
        //     {
        //         driverLicenseCredential, driverLicenseCredentialClone, universityCredential
        //     },
        //     new[] { driverLicenseInputDescriptor, universityInputDescriptor });
        //
        // // Assert
        // credentialCandidatesArray.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_Fields_Are_Fulfilled()
    {
        // // Arrange
        // var driverLicenseClaims = new Dictionary<string, string>
        // {
        //     { "id", "123" },
        //     { "issuer", "did:example:gov" },
        //     { "dateOfBirth", "01/01/2000" }
        // };
        //
        // var employeeCredential = CreateCredential(driverLicenseClaims);
        //
        // var driverLicenseInputDescriptor = CreateInputDescriptor(
        //     CreateConstraints(new[]
        //     {
        //         CreateField("$.id"), CreateField("$.issuer"),
        //         CreateField("$.dateOfBirth"), CreateField("$.name")
        //     }),
        //     new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
        //     Guid.NewGuid().ToString(),
        //     "EU Driver's License",
        //     "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");
        //
        // var sdJwtVcHolderService = CreateOid4VpClientService();
        //
        // // Act
        // var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
        //     new[] { employeeCredential },
        //     new[] { driverLicenseInputDescriptor });
        //
        // // Assert
        // credentialCandidatesArray.Should().BeEmpty();
    }

    [Fact]
    public async Task Cant_Get_Credential_Candidates_When_Not_All_Filters_Are_Fulfilled()
    {
        // // Arrange
        // var driverLicenseClaims = new Dictionary<string, string>
        // {
        //     { "id", "123" },
        //     { "issuer", "did:example:gov" },
        //     { "dateOfBirth", "01/01/2000" }
        // };
        //
        // var driverLicenseCredential = CreateCredential(driverLicenseClaims);
        //
        // var idFilter = new Filter();
        // idFilter.PrivateSet(x => x.Type, "string");
        // idFilter.PrivateSet(x => x.Const, "326");
        //
        // var driverLicenseInputDescriptor = CreateInputDescriptor(
        //     CreateConstraints(new[]
        //     {
        //         CreateField("$.id", idFilter), CreateField("$.issuer"), CreateField("$.dateOfBirth")
        //     }),
        //     new Dictionary<string, Format> { {"vc+sd-jwt", CreateFormat(new[] { "ES256" }) }},
        //     Guid.NewGuid().ToString(),
        //     "EU Driver's License",
        //     "We can only accept digital driver's licenses issued by national authorities of member states or trusted notarial auditors.");
        //
        // var sdJwtVcHolderService = CreateOid4VpClientService();
        //
        // // Act
        // var credentialCandidatesArray = await sdJwtVcHolderService.FindCredentialCandidates(
        //     new[] { driverLicenseCredential },
        //     new[] { driverLicenseInputDescriptor });
        //
        // // Assert
        // credentialCandidatesArray.Should().BeEmpty();
    }

    [Fact]
    public async Task Can_Create_Sd_Jwt_Presentation_Without_Key_Binding()
    {
        // // Arrange
        // var credential = new SdJwtRecord();
        //
        // const string issuerSignedJwt = "eyJhbGciOiAiRVMyNTYifQ.eyJfc2QiOiBbIkNyUWU3UzVrcUJBSHQtbk1ZWGdjNmJkdDJTSDVhVFkxc1VfTS1QZ2tqUEkiLCAiSnpZakg0c3ZsaUgwUjNQeUVNZmVadTZKdDY5dTVxZWhabzdGN0VQWWxTRSIsICJQb3JGYnBLdVZ1Nnh5bUphZ3ZrRnNGWEFiUm9jMkpHbEFVQTJCQTRvN2NJIiwgIlRHZjRvTGJnd2Q1SlFhSHlLVlFaVTlVZEdFMHc1cnREc3JaemZVYW9tTG8iLCAiWFFfM2tQS3QxWHlYN0tBTmtxVlI2eVoyVmE1TnJQSXZQWWJ5TXZSS0JNTSIsICJYekZyendzY002R242Q0pEYzZ2Vks4QmtNbmZHOHZPU0tmcFBJWmRBZmRFIiwgImdiT3NJNEVkcTJ4Mkt3LXc1d1BFemFrb2I5aFYxY1JEMEFUTjNvUUw5Sk0iLCAianN1OXlWdWx3UVFsaEZsTV8zSmx6TWFTRnpnbGhRRzBEcGZheVF3TFVLNCJdLCAiaXNzIjogImh0dHBzOi8vZXhhbXBsZS5jb20vaXNzdWVyIiwgImlhdCI6IDE2ODMwMDAwMDAsICJleHAiOiAxODgzMDAwMDAwLCAic3ViIjogInVzZXJfNDIiLCAibmF0aW9uYWxpdGllcyI6IFt7Ii4uLiI6ICJwRm5kamtaX1ZDem15VGE2VWpsWm8zZGgta284YUlLUWM5RGxHemhhVllvIn0sIHsiLi4uIjogIjdDZjZKa1B1ZHJ5M2xjYndIZ2VaOGtoQXYxVTFPU2xlclAwVmtCSnJXWjAifV0sICJfc2RfYWxnIjogInNoYS0yNTYiLCAiY25mIjogeyJqd2siOiB7Imt0eSI6ICJFQyIsICJjcnYiOiAiUC0yNTYiLCAieCI6ICJUQ0FFUjE5WnZ1M09IRjRqNFc0dmZTVm9ISVAxSUxpbERsczd2Q2VHZW1jIiwgInkiOiAiWnhqaVdXYlpNUUdIVldLVlE0aGJTSWlyc1ZmdWVjQ0U2dDRqVDlGMkhaUSJ9fX0.kmx687kUBiIDvKWgo2Dub-TpdCCRLZwtD7TOj4RoLsUbtFBI8sMrtH2BejXtm_P6fOAjKAVc_7LRNJFgm3PJhg";
        // const string givenNameDisclosure = "WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgImdpdmVuX25hbWUiLCAiSm9obiJd";
        // const string familyNameDisclosure = "WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgImZhbWlseV9uYW1lIiwgIkRvZSJd";
        // const string nationalityDisclosure = "WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgIm5hdGlvbmFsaXR5IiwgImdlcm1hbiJd"; 
        //
        // credential.PrivateSet(x => x.EncodedIssuerSignedJwt, issuerSignedJwt);
        // credential.PrivateSet(x => x.Disclosures, ImmutableArray.Create<string>(nationalityDisclosure, familyNameDisclosure, givenNameDisclosure));
        //
        // var claimsToDisclose = new[] { "given_name", "family_name" };
        //
        // const string expected = issuerSignedJwt + "~" + givenNameDisclosure + "~" + familyNameDisclosure;
        //
        // var service = CreateOid4VpClientService();
        //
        // // Act
        // var presentation = await service.CreatePresentation(credential, claimsToDisclose);
        //
        // // Assert
        // presentation.Should().BeEquivalentTo(expected);
    }
    
    private static IOid4VpClientService CreateOid4VpClientService()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var sdJwtVcHolderService = new Mock<ISdJwtVcHolderService>();
        var oid4VpHaipClient = new Mock<IOid4VpHaipClient>();
        var oid4VpRecordservice = new Mock<IOid4VpRecordService>();

        return new Oid4VpClientService(
            httpClientFactory.Object, sdJwtVcHolderService.Object, oid4VpHaipClient.Object, oid4VpRecordservice.Object);
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
    
    private static Constraints CreateConstraints(Field[] fields)
    {
        var constraints = new Constraints();
        constraints.PrivateSet(x => x.Fields, fields);

        return constraints;
    }
    
    private static SdJwtRecord CreateCredential(Dictionary<string, string> claims)
    {
        var record = new SdJwtRecord
        {
            Id = Guid.NewGuid().ToString(),
            Claims = claims
        };

        return record;
    }
}
