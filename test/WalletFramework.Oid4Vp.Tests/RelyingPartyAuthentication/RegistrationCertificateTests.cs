using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.RegistrationCertificate;
using WalletFramework.Oid4Vp.Tests.RelyingPartyAuthentication.Samples;

namespace WalletFramework.Oid4Vp.Tests.RelyingPartyAuthentication;

public class RegistrationCertificateTests
{
    [Fact]
    public void Can_Parse_Registration_Certificate_Data()
    {
        var sut = RegistrationCertificate.FromJwtTokenStr(RegistrationCertificateSamples.JwtStrSample);
        sut.Match(
            _ =>
            {
                // TODO: Check properties
            },
            errors =>
            {
                Assert.Fail("RegistrationCertificate Validation failed");
            });
    }
}
