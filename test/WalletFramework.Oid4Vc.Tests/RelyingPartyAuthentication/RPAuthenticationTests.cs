using FluentAssertions;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using WalletFramework.Oid4Vc.Tests.RelyingPartyAuthentication.Samples;

namespace WalletFramework.Oid4Vc.Tests.RelyingPartyAuthentication;

public class RpAuthenticationTests
{
    [Fact]
    public void AuthorizationRequest_WithoutOverasking_HasValidRegistrationCertificateValidationResult()
    {
        var requestObject = RpAuthSamples.ValidSignedRequestSample;

        var sut = OverAskingValidationResult.Validate(requestObject);

        sut.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AuthorizationRequest_WithOverasking_HasInvalidRegistrationCertificateValidationResult()
    {
        var requestObject = RpAuthSamples.OverAskingSignedRequestSample;

        var sut = OverAskingValidationResult.Validate(requestObject);

        sut.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Untrusted_Access_Certificate_results_in_Trust_Level_Abort()
    {
        // TODO: Implement this test
    }

    [Fact]
    public void Valid_Request_results_in_Trust_Level_Green()
    {
        var requestObject = RpAuthSamples.ValidSignedRequestSample;
        var rpRegistrarCert = RpAuthSamples.GetRpRegistrarCertificateSample();

        var sut = RpAuthResult.ValidateRequestObject(requestObject, rpRegistrarCert);

        sut.TrustLevel.Should().Be(RpTrustLevel.Green);
    }
}
