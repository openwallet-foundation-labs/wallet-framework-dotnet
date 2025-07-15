using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.DcApi;

public class DcApiRequestBatchTests
{
    [Fact]
    public void Unsigned_Request_Can_Be_Processed()
    {
        // Arrange
        var validJson = DcApiSamples.ValidDcApiUnsignedRequestBatchJson;
    
        // Act
        var result = DcApiRequestBatch.From(validJson);
    
        // Assert
        result.Match(
            dcApiRequestBatch =>
            {
                dcApiRequestBatch.Requests.Should().HaveCount(1);
                
                var requestItem = dcApiRequestBatch.Requests[0];
                requestItem.Protocol.Should().Be(DcApiConstants.UnsignedProtocol);
                
                var dcApiRequest = requestItem.Data;
                dcApiRequest.Nonce.Should().Be("cQAgOKI-5dXxyhKJI38QX-d_qGLxXgn_1wSYmzeCDTQ");
                dcApiRequest.ResponseMode.Should().Be("dc_api");
                
                dcApiRequest.DcqlQuery.Should().NotBeNull();
                dcApiRequest.DcqlQuery!.CredentialQueries.Should().HaveCount(1);
                
                var credentialQuery = dcApiRequest.DcqlQuery.CredentialQueries[0];
                credentialQuery.Id.AsString().Should().Be("cred1");
                credentialQuery.Format.Should().Be("mso_mdoc");
                credentialQuery.Meta!.Doctype.Should().Be("org.iso.18013.5.1.mDL");
                credentialQuery.Claims.Should().HaveCount(2);
                
                var firstClaim = credentialQuery.Claims![0];
                firstClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "family_name");
                
                var secondClaim = credentialQuery.Claims![1];
                secondClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "given_name");
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }

    [Fact]
    public void Signed_Request_Can_Be_Processed()
    {
        // Arrange
        var validJson = DcApiSamples.ValidDcApiSignedRequestBatchJson;
    
        // Act
        var result = DcApiRequestBatch.From(validJson);
        
        // Assert
        result.Match(
            dcApiRequestBatch =>
            {
                dcApiRequestBatch.Requests.Should().HaveCount(1);
                
                var requestItem = dcApiRequestBatch.Requests[0];
                requestItem.Protocol.Should().Be(DcApiConstants.SignedProtocol);
                
                var dcApiRequest = requestItem.Data;
                dcApiRequest.Nonce.Should().Be("dhGdw3HQ0dHfOZlLkEB78JIuPZcn-._~");
                dcApiRequest.ResponseMode.Should().Be("dc_api.jwt");
                
                dcApiRequest.DcqlQuery.Should().NotBeNull();
                dcApiRequest.DcqlQuery!.CredentialQueries.Should().HaveCount(1);
                
                var credentialQuery = dcApiRequest.DcqlQuery.CredentialQueries[0];
                credentialQuery.Id.AsString().Should().Be("cred1");
                credentialQuery.Format.Should().Be("mso_mdoc");
                credentialQuery.Meta!.Doctype.Should().Be("org.iso.18013.5.1.mDL");
                credentialQuery.Claims.Should().HaveCount(2);
                
                var firstClaim = credentialQuery.Claims![0];
                firstClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "family_name");
                
                var secondClaim = credentialQuery.Claims![1];
                secondClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "given_name");
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }

    [Fact]
    public void GetFirstVpRequest_Returns_First_Request()
    {
        // Arrange
        var validJson = DcApiSamples.ValidDcApiUnsignedRequestBatchJson;
        var result = DcApiRequestBatch.From(validJson);
        
        // Act & Assert
        result.Match(
            dcApiRequestBatch =>
            {
                var firstVpRequest = dcApiRequestBatch.GetFirstVpRequest();
                
                firstVpRequest.IsSome.Should().BeTrue();
                firstVpRequest.Match(
                    request => request.Protocol.Should().Be(DcApiConstants.UnsignedProtocol),
                    () => Assert.Fail("Expected to find a request with protocol 'openid4vp'")
                );
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }

    [Fact]
    public void GetFirstVpRequest_Returns_None_When_Protocol_Not_Found()
    {
        // Arrange
        var modifiedJson = DcApiSamples.ValidDcApiUnsignedRequestBatchJson.Replace($"\"protocol\":\"{DcApiConstants.UnsignedProtocol}\"", "\"protocol\":\"non_existing_protocol\"");
        var result = DcApiRequestBatch.From(modifiedJson);
        
        // Act & Assert
        result.Match(
            dcApiRequestBatch =>
            {
                var firstVpRequest = dcApiRequestBatch.GetFirstVpRequest();
                firstVpRequest.IsNone.Should().BeTrue();
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }
} 
