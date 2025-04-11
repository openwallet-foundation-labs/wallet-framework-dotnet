using FluentAssertions;
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
}
