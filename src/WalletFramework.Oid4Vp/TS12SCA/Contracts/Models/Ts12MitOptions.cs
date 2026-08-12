using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.TS12SCA.Internal;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12MitOptions(
    Option<bool> AmountVariable,
    Option<decimal> MinAmount,
    Option<decimal> MaxAmount,
    Option<decimal> TotalAmount,
    Option<decimal> InitialAmount,
    Option<int> InitialAmountNumber,
    Option<decimal> Apr)
{
    public static Validation<Ts12MitOptions> FromJObject(JObject jObject) =>
        from amountVariable in jObject.GetOptionalBoolean("amount_variable")
        from minAmount in jObject.GetOptionalDecimal("min_amount")
        from maxAmount in jObject.GetOptionalDecimal("max_amount")
        from totalAmount in jObject.GetOptionalDecimal("total_amount")
        from initialAmount in jObject.GetOptionalDecimal("initial_amount")
        from initialAmountNumber in jObject.GetOptionalInt("initial_amount_number")
        from apr in jObject.GetOptionalDecimal("apr")
        select new Ts12MitOptions(
            amountVariable,
            minAmount,
            maxAmount,
            totalAmount,
            initialAmount,
            initialAmountNumber,
            apr);
}
