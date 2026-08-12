using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vp.TS12SCA.Internal;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12Recurrence(
    Option<Ts12Date> StartDate,
    Option<Ts12Date> EndDate,
    Option<int> Number,
    string Frequency,
    Option<Ts12MitOptions> MitOptions)
{
    public static Validation<Ts12Recurrence> FromJObject(JObject jObject) =>
        from startDate in jObject.GetOptional("start_date", Ts12Date.FromJToken)
        from endDate in jObject.GetOptional("end_date", Ts12Date.FromJToken)
        from number in jObject.GetOptionalInt("number")
        from frequencyToken in jObject.GetByKey("frequency")
        from mitOptions in jObject.GetOptionalObject("mit_options", Ts12MitOptions.FromJObject)
        select new Ts12Recurrence(startDate, endDate, number, frequencyToken.ToString(), mitOptions);
}
