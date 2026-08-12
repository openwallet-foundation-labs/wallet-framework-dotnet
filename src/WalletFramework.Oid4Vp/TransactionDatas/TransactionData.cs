using OneOf;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vp.Payment;
using WalletFramework.Oid4Vp.Qes.Authorization;
using WalletFramework.Oid4Vp.Qes.CertCreation;
using WalletFramework.Oid4Vp.TS12SCA;
using WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

namespace WalletFramework.Oid4Vp.TransactionDatas;

public class TransactionData(
    OneOf<PaymentTransactionData, QesAuthorizationTransactionData, QCertCreationTransactionData, Ts12PaymentTransactionData> input)
    : OneOfBase<PaymentTransactionData, QesAuthorizationTransactionData, QCertCreationTransactionData, Ts12PaymentTransactionData>(input)
{
    public static Validation<TransactionData> FromBase64Url(Base64UrlString base64UrlString) =>
        from jObject in base64UrlString.DecodeToJObject()
        from properties in TransactionDataProperties.FromJObject(jObject, base64UrlString)
        from transactionData in properties.Type.Value switch
        {
            TransactionDataTypeValue.Payment => PaymentTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.Qes => QesAuthorizationTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.QCertCreation => QCertCreationTransactionData.FromJObject(jObject, properties),
            TransactionDataTypeValue.Ts12Payment => Ts12PaymentTransactionData.FromJObject(jObject, properties),
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

    public static TransactionData WithTs12PaymentTransactionData(Ts12PaymentTransactionData input)
    {
        return new TransactionData(input);
    }
}
