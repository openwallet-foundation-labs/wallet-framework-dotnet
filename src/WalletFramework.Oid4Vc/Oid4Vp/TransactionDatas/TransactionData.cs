using LanguageExt;
using OneOf;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Payment;
using WalletFramework.Oid4Vc.Qes.Authorization;
using WalletFramework.Oid4Vc.Qes.CertCreation;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public class TransactionData(
    OneOf<PaymentTransactionData, QesAuthorizationTransactionData, QCertCreationTransactionData> input)
    : OneOfBase<PaymentTransactionData, QesAuthorizationTransactionData, QCertCreationTransactionData>(input)
{
    public static Validation<TransactionData> FromBase64Url(Base64UrlString base64UrlString) =>
        from jObject in base64UrlString.DecodeToJObject()
        from properties in TransactionDataProperties.FromJObject(jObject, base64UrlString)
        from transactionData in properties.Type.Value switch
        {
            TransactionDataTypeValue.Payment => PaymentTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.Qes => QesAuthorizationTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.QCertCreation => QCertCreationTransactionData.FromJObject(jObject, properties),
            _ => throw new InvalidOperationException()
        }
        select transactionData;

    public static TransactionData WithPaymentTransactionData(PaymentTransactionData input)
    {
        return new TransactionData(input);
    }
    
    public static TransactionData WithQesAuthorizationTransactionData(QesAuthorizationTransactionData input)
    {
        return new TransactionData(input);
    }
    
    public static TransactionData WithQCertCreationTransactionData(QCertCreationTransactionData input)
    {
        return new TransactionData(input);
    }
}

public static class TransactionDataFun
{
    public static TransactionDataType GetTransactionDataType(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().Type;

    public static IEnumerable<TransactionDataHashesAlg> GetHashesAlg(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().TransactionDataHashesAlg;
    
    public static Base64UrlString GetEncoded(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().Encoded;

    public static Option<PresentationCandidate> FindCandidateForTransactionData(
        this IEnumerable<PresentationCandidate> candidates,
        TransactionData transactionData) => candidates.FirstOrDefault(candidate =>
        transactionData.GetTransactionDataProperties().CredentialIds.Select(id => id.AsString)
            .Contains(candidate.Identifier));

    private static TransactionDataProperties GetTransactionDataProperties(this TransactionData transactionData) =>
        transactionData.Match(
            payment => payment.TransactionDataProperties,
            qes => qes.TransactionDataProperties,
            qcert => qcert.TransactionDataProperties);

}
