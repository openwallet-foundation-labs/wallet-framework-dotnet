using FluentAssertions;
using Hyperledger.Aries.Storage;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Uri;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Records;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.AuthFlow.Samples;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Issuer.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.AuthFlow;

public class AuthFlowSessionRecordTests
{
    [Fact]
    public void Can_Encode_To_Json()
    {
        // Arrange
        var clientOptions = new ClientOptions
        {
            ClientId = "i can write anything", 
            WalletIssuer = "i can write anything",
            RedirectUri = "i can write anything"
        };

        var issuerMetadata = IssuerMetadataSample.DecodedDraft14AndLower;
        
        var authorizationServerMetadata = new AuthorizationServerMetadata
        {
            Issuer = "i can write anything",
            TokenEndpoint = "i can write anything",
            JwksUri = "i can write anything",
            AuthorizationEndpoint = "i can write anything",
            ResponseTypesSupported = new[] { "i can write anything" },
        };
        
        var credentialConfigurationId = CredentialConfigurationId
            .ValidCredentialConfigurationId(IssuerMetadataSample.MdocConfigurationId.ToString())
            .UnwrapOrThrow(new InvalidOperationException());
        
        var authorizationData = new AuthorizationData(
            clientOptions,
            issuerMetadata,
            authorizationServerMetadata,
            Option<OAuthToken>.None, 
            new List<CredentialConfigurationId> { credentialConfigurationId });
        
        var authorizationCodeParameters = new AuthorizationCodeParameters("hello", "world");
        
        var sessionId = AuthFlowSessionState.CreateAuthFlowSessionState();
        var record = new AuthFlowSessionRecord(authorizationData, authorizationCodeParameters, sessionId);
        
        // Act
        var recordSut = JObject.FromObject(record);
        var tagsSut = JObject.FromObject(record.Tags);
        
        // Assert
        recordSut[nameof(RecordBase.Id)]!.ToString().Should().Be(record.Id);
        tagsSut[nameof(AuthFlowSessionRecord.AuthFlowSessionState)] = record.AuthFlowSessionState.ToString();
    }

    [Fact]
    public void Can_Decode_From_Json()
    {
        // Arrange
        var json = AuthFlowSamples.AuthFlowSessionRecordJson;
        
        // Act
        var record = json.ToObject<AuthFlowSessionRecord>();
        
        // Assert
        record.Should().NotBeNull();
        record!.Id.Should().Be(json[nameof(RecordBase.Id)]!.ToString());
        record.AuthorizationData.IssuerMetadata.CredentialIssuer.ToString().Should().Be(IssuerMetadataSample.CredentialIssuer.ToStringWithoutTrail());
    }
}
