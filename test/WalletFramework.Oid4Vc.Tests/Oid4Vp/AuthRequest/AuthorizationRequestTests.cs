using FluentAssertions;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthRequest.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthRequest;

public class AuthorizationRequestTests
{
    [Fact]
    public void Can_Parse_Authorization_Request_Without_Attachments()
    {
        var json = AuthorizationRequestServiceTestsDataProvider.GetJsonForTestCase();
        var authRequest = AuthorizationRequest.CreateAuthorizationRequest(json);

        authRequest.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void Can_Parse_Authorization_Request_With_Attachments()
    {
        var json = AuthorizationRequestServiceTestsDataProvider.GetJsonForTestCase();
        var authRequest = AuthorizationRequest.CreateAuthorizationRequest(json);

        authRequest.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void Can_Parse_Null_ClaimPath_Component_As_Array_Wildcard()
    {
        var json = """
                   {
                     "client_id": "x509_san_dns:funke-wallet.de",
                     "response_uri": "https://funke-wallet.de/oid4vp/response",
                     "response_mode": "direct_post",
                     "nonce": "860303407528324526871313",
                     "dcql_query": {
                       "credentials": [
                         {
                           "id": "pid",
                           "format": "dc+sd-jwt",
                           "meta": {
                             "vct_values": [
                               "https://demo.pid-issuer.bundesdruckerei.de/credentials/pid/1.0"
                             ]
                           },
                           "claims": [
                             {
                               "path": ["degrees", null, "type"]
                             }
                           ]
                         }
                       ]
                     }
                   }
                   """;

        var authorizationRequest = AuthorizationRequest.CreateAuthorizationRequest(json).UnwrapOrThrow();
        var claimPath = authorizationRequest.DcqlQuery.CredentialQueries[0].Claims![0].Path;

        claimPath.ToJsonPath().ToString().Should().Be("$.degrees[*].type");
        claimPath.GetPathComponents()[1]
            .Match(
                _ => false,
                _ => false,
                _ => true)
            .Should()
            .BeTrue();
    }
}
