using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Models;
using static WalletFramework.Oid4Vp.Tests.AuthRequest.AuthorizationRequestServiceFactory;
using static WalletFramework.Oid4Vp.Tests.AuthRequest.Samples.X509HashSamples;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest;

public class X509HashRequestProcessingTests
{
    private const string RequestUri = "https://verifier.example.com/request";

    [Fact]
    public async Task A_Signed_Request_With_A_Matching_Certificate_Hash_Is_Processed_Successfully()
    {
        var service = CreateServiceReturning(SignedRequestObjectWithMatchingCertificateHash());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_Signed_Request_With_A_Mismatched_Certificate_Hash_Is_Cancelled_Without_Throwing()
    {
        var service = CreateServiceReturning(SignedRequestObjectWithMismatchedCertificateHash());

        var result = await service.GetAuthorizationRequest(ReferenceUri());

        result.IsSuccess.Should().BeFalse();
    }

    private static AuthorizationRequestUri ReferenceUri()
    {
        var uri = new Uri($"openid4vp://?client_id={ClientIdScheme.X509HashScheme}:placeholder&request_uri={Uri.EscapeDataString(RequestUri)}");
        return AuthorizationRequestUri.FromUri(uri).UnwrapOrThrow();
    }
}
