using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Tests.QCertCreation.Samples;
using WalletFramework.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vp.Tests.QCertCreation;

public class QCertCreationTests
{
    [Theory]
    [InlineData(nameof(QCertCreationTransactionDataSamples.QCertJsonSample))]
    [InlineData(nameof(QCertCreationTransactionDataSamples.CscQCertJsonSample))]
    public void Can_Parse_QCertCreation_Transaction_Data(string sampleName)
    {
        var jsonSample = sampleName switch
        {
            nameof(QCertCreationTransactionDataSamples.QCertJsonSample) => QCertCreationTransactionDataSamples.QCertJsonSample,
            nameof(QCertCreationTransactionDataSamples.CscQCertJsonSample) => QCertCreationTransactionDataSamples.CscQCertJsonSample,
            _ => throw new ArgumentException($"Unknown sample name: {sampleName}")
        };

        var sample = QCertCreationTransactionDataSamples.ToBase64UrlString(jsonSample);

        var sut = TransactionData.FromBase64Url(sample);
        sut.Match(
            _ =>
            {
                // TODO: Check properties
            },
            errors =>
            {
                Assert.Fail($"QCertCreationTransactionData Validation failed for sample: {sampleName}");
            });
    }
}
