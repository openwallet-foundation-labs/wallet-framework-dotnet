using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Tests.Signatures.Samples;

namespace WalletFramework.Oid4Vc.Tests.Signatures;

public class SignatureTests
{
    [Fact]
    public void Can_Parse_QCertCreation_Transaction_Data()
    {
        var sample = SignatureTransactionDataSamples.ToBase64UrlString();

        var sut = TransactionData.FromBase64Url(sample);
        sut.Match(
            _ =>
            {
                // TODO: Check properties
            },
            errors =>
            {
                Assert.Fail("QCertCreationTransactionData Validation failed");
            });
    }
}
