using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;

public record CredentialSetOption(
    [property: JsonConverter(typeof(CredentialQueryIdListJsonConverter))]
    IReadOnlyList<CredentialQueryId> Ids)
{
    public static Validation<CredentialSetOption> FromJArray(JArray array) =>
        from jValues in array.TraverseAll(token => token.ToJValue())
        from queryIds in FromStrings(jValues.Select(value => value.ToString(CultureInfo.InvariantCulture)))
        select queryIds;

    public static Validation<CredentialSetOption> FromStrings(IEnumerable<string> strings) =>
        from queryIds in strings.TraverseAll(CredentialQueryId.Create)
        select new CredentialSetOption(queryIds.ToArray());
} 
