using LanguageExt;
using OneOf;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Payment;
using WalletFramework.Oid4Vc.Qes;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public class TransactionData(
    OneOf<PaymentTransactionData, QesAuthorizationTransactionData> input
    ) : OneOfBase<PaymentTransactionData, QesAuthorizationTransactionData>(input)
{
    public static Validation<TransactionData> FromBase64Url(Base64UrlString base64UrlString) =>
        from jObject in base64UrlString.DecodeToJObject()
        from properties in TransactionDataProperties.FromJObject(jObject, base64UrlString)
        from transactionData in properties.Type.Value switch
        {
            TransactionDataTypeValue.Payment => PaymentTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.Qes => QesAuthorizationTransactionData.FromJObject(jObject, properties),
            _ => throw new InvalidOperationException()
        }
        select transactionData;
}

public static class TransactionDataFun
{
    public static TransactionDataType GetTransactionDataType(this TransactionData transactionData) =>
        transactionData.Match(
            payment => payment.TransactionDataProperties.Type,
            qes => qes.TransactionDataProperties.Type);

    public static IEnumerable<TransactionDataHashesAlg> GetHashesAlg(this TransactionData transactionData) => 
        transactionData.Match(
            payment => payment.TransactionDataProperties.TransactionDataHashesAlg,
            qes => qes.TransactionDataProperties.TransactionDataHashesAlg);
    
    public static Base64UrlString GetEncoded(this TransactionData transactionData) =>
        transactionData.Match(
            payment => payment.TransactionDataProperties.Encoded,
            qes => qes.TransactionDataProperties.Encoded);

    public static Option<PresentationCandidate> FindCandidateForTransactionData(
        this IEnumerable<PresentationCandidate> candidates,
        TransactionData transactionData)
    {
        var credentialIds = transactionData.Match(
            payment => payment.TransactionDataProperties.CredentialIds.Select(id => id.AsString),
            qes => qes.TransactionDataProperties.CredentialIds.Select(id => id.AsString));
        
        return candidates.FirstOrDefault(candidate => credentialIds.Contains(candidate.Identifier));
    }
}
