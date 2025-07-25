using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Qes.Authorization;

public record DocumentDigest(
    DocumentDigestLabel Label,
    Option<Uri> DocumentLocationUri)
{
    public static Validation<DocumentDigest> FromJObject(JToken jObject)
    {
        var labelValidation =
            from labelToken in jObject.GetByKey("label")
            from label in DocumentDigestLabel.FromString(labelToken.ToString())
            select label;
        
        var uriValidation = 
            from documentLocationUri in jObject.GetByKey("documentLocation_uri")
            select new Uri(documentLocationUri.ToString());
        
        var hrefValidation = 
            from documentLocationUri in jObject.GetByKey("href")
            select new Uri(documentLocationUri.ToString());

        return
            from label in labelValidation
            let uriOption = hrefValidation.ToOption().Match(
                hrefUri => hrefUri,
                () => uriValidation.ToOption())
            select new DocumentDigest(label, uriOption);
    }
}
