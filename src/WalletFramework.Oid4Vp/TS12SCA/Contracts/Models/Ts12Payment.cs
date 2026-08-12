using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vp.TS12SCA.Internal;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12Payment(
    Ts12TransactionId TransactionId,
    Option<Ts12DateTime> DateTime,
    Ts12Payee Payee,
    Option<Ts12Pisp> Pisp,
    Option<Ts12Date> ExecutionDate,
    string Currency,
    decimal Amount,
    Option<bool> AmountEstimated,
    Option<bool> AmountEarmarked,
    Option<bool> SctInst,
    Option<Ts12Recurrence> Recurrence)
{
    public static Validation<Ts12Payment> FromJObject(JObject jObject) =>
        from transactionIdToken in jObject.GetByKey("transaction_id")
        from transactionId in Ts12TransactionId.FromJToken(transactionIdToken)
        from dateTime in jObject.GetOptional("date_time", Ts12DateTime.FromJToken)
        from payeeToken in jObject.GetByKey("payee")
        from payeeObject in payeeToken.ToJObject()
        from payee in Ts12Payee.FromJObject(payeeObject)
        from pisp in jObject.GetOptionalObject("pisp", Ts12Pisp.FromJObject)
        from executionDate in jObject.GetOptional("execution_date", Ts12Date.FromJToken)
        from currencyToken in jObject.GetByKey("currency")
        from amountToken in jObject.GetByKey("amount")
        from amount in Ts12JsonFun.ToDecimal(amountToken, "amount")
        from amountEstimated in jObject.GetOptionalBoolean("amount_estimated")
        from amountEarmarked in jObject.GetOptionalBoolean("amount_earmarked")
        from sctInst in jObject.GetOptionalBoolean("sct_inst")
        from recurrence in jObject.GetOptionalObject("recurrence", Ts12Recurrence.FromJObject)
        select new Ts12Payment(
            transactionId,
            dateTime,
            payee,
            pisp,
            executionDate,
            currencyToken.ToString(),
            amount,
            amountEstimated,
            amountEarmarked,
            sctInst,
            recurrence);
}
