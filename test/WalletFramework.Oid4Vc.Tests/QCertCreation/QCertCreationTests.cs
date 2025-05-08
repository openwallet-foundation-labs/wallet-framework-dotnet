using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Tests.QCertCreation.Samples;

namespace WalletFramework.Oid4Vc.Tests.QCertCreation;

public class QCertCreationTests
{
    [Fact]
    public void Can_Parse_QCertCreation_Transaction_Data()
    {
        var sample = QCertCreationTransactionDataSamples.GetBase64UrlStringSample();

        var sut = TransactionData.FromBase64Url(sample);
        sut.Match(
            _ =>
            {
                // TODO: Check properties
            },
            errors =>
            {
                Assert.Fail("QCertCreationtTransactionData Validation failed");
            });
    }
}
