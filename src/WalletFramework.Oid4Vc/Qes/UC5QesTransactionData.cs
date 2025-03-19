using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Qes;

public record Uc5QesTransactionData(
    Base64UrlString Encoded,
    List<DocumentDigest> DocumentDigests)
{
    public static Validation<IEnumerable<Uc5QesTransactionData>> FromJArray(JArray jArray)
    {
        var base64UrlStringsValidation = jArray.TraverseAll(token => Base64UrlString.FromString(token.ToString()));

        return 
            from base64UrlStrings in base64UrlStringsValidation
            from txData in base64UrlStrings.TraverseAll(base64UrlString =>
            {
                return 
                    from jObject in base64UrlString.DecodeToJObject()
                    from uc5TxData in FromJObject(jObject, base64UrlString)
                    select uc5TxData;
            })
            select txData;
    }

    public static Validation<Uc5QesTransactionData> FromJObject(JObject jObject, Base64UrlString encoded) =>
        from jToken in jObject.GetByKey("documentDigests")
        from jArray in jToken.ToJArray()
        from documentDigests in jArray.TraverseAll(DocumentDigest.FromJObject)
        select new Uc5QesTransactionData(encoded, documentDigests.ToList());
}
