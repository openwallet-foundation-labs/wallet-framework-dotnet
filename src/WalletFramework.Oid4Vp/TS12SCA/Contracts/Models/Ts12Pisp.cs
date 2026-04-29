using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vp.TS12SCA.Contracts.Models;

public sealed record Ts12Pisp(
    string LegalName,
    string BrandName,
    string DomainName)
{
    public static Validation<Ts12Pisp> FromJObject(JObject jObject) =>
        from legalName in jObject.GetByKey("legal_name")
        from brandName in jObject.GetByKey("brand_name")
        from domainName in jObject.GetByKey("domain_name")
        select new Ts12Pisp(legalName.ToString(), brandName.ToString(), domainName.ToString());
}
