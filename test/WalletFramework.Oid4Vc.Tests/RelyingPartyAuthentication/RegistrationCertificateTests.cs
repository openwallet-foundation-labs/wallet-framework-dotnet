using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using WalletFramework.Oid4Vc.Tests.RelyingPartyAuthentication.Samples;

namespace WalletFramework.Oid4Vc.Tests.RelyingPartyAuthentication;

public class RegistrationCertificateTests
{
    [Fact]
    public void Can_Parse_Registration_Certificate_Data()
    {
        var sut = RegistrationCertificate.FromJObject(RegistrationCertificateSamples.JsonSample);
        sut.Match(
            _ =>
            {
                // TODO: Check properties
            },
            errors =>
            {
                Assert.Fail("PaymentTransactionData Validation failed");
            });
    }
}
