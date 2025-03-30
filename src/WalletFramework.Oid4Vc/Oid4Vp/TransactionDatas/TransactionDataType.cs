using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public enum TransactionDataTypeValue
{
    Payment,
    Qes
}

public static class TransactionDataTypeValueFun
{
    public static string AsString(this TransactionDataTypeValue value) =>
        value switch
        {
            TransactionDataTypeValue.Payment => SupportedTransactionDataTypeConstants.Payment,
            TransactionDataTypeValue.Qes => SupportedTransactionDataTypeConstants.Qes,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
}

public static class SupportedTransactionDataTypeConstants
{
    public const string Payment = "payment_data";

    public const string Qes = "qes_authorization";
}

public readonly struct TransactionDataType
{
    public TransactionDataTypeValue Value { get; }

    private TransactionDataType(TransactionDataTypeValue value) => Value = value;

    public string AsString() => Value.AsString();

    public static implicit operator string(TransactionDataType type) => type.AsString();

    public static Validation<TransactionDataType> FromJToken(JToken jToken)
    {
        var type = jToken.ToString();

        if (string.IsNullOrWhiteSpace(type))
        {
            return new InvalidTransactionDataError("The transaction data type is null or empty");
        }

        return type switch
        {
            SupportedTransactionDataTypeConstants.Payment => new TransactionDataType(TransactionDataTypeValue.Payment),
            SupportedTransactionDataTypeConstants.Qes => new TransactionDataType(TransactionDataTypeValue.Qes),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
