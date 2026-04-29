using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vp.TS12SCA.Internal;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12Payee(
    string Name,
    string Id,
    Option<string> Logo,
    Option<string> Website)
{
    public static Validation<Ts12Payee> FromJObject(JObject jObject) =>
        from nameToken in jObject.GetByKey("name")
        from idToken in jObject.GetByKey("id")
        from logo in jObject.GetOptionalString("logo")
        from website in jObject.GetOptionalString("website")
        select new Ts12Payee(nameToken.ToString(), idToken.ToString(), logo, website);
}
