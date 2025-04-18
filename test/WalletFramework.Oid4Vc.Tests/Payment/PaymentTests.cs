using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Tests.Payment.Samples;

namespace WalletFramework.Oid4Vc.Tests.Payment;

public class PaymentTests
{
    [Fact]
    public void Can_Parse_Payment_Transaction_Data()
    {
        var sample = PaymentTransactionDataSamples.GetBase64UrlStringSample();

        var sut = TransactionData.FromBase64Url(sample);
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
