using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Errors;
using WalletFramework.MdocLib;
using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.MdocLib.Elements;
using WalletFramework.SdJwtLib.Models;
using static WalletFramework.Core.ClaimPaths.ClaimPathSelectionFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.ClaimPaths;

public static class ClaimPathFun
{
    public static Validation<ClaimPathSelection> ProcessWith(this ClaimPath path, string json)
    {
        JObject jObject;
        try
        {
            jObject = JObject.Parse(json);
        }
        catch (Exception e)
        {
            return new InvalidJsonError(json, e);
        }

        var components = path.GetPathComponents();
        return components.Aggregate(
            ClaimPathSelection.Create([jObject]),
            (validation, component) => validation.OnSuccess(selection =>
            {
                return component.Match(
                    s => SelectObjectKey(selection, s),
                    i => SelectArrayIndex(selection, i),
                    _ => SelectAllArrayElements(selection)
                );
            })
        );
    }
    
    public static Validation<ClaimPathSelection> ProcessWith(this ClaimPath path, SdJwtDoc sdJwtDoc)
    {
        var jsonStr = sdJwtDoc.UnsecuredPayload.ToString();
        return path.ProcessWith(jsonStr);
    }

    public static Validation<ClaimPathSelection> ProcessWith(this ClaimPath path, Mdoc mdoc)
    {
        var components = path.GetPathComponents();
        if (components.Count != 2 || !components[0].IsKey || !components[1].IsKey)
            return new UnknownComponentError();

        var nsStr = components[0].AsKey();
        var elemStr = components[1].AsKey();
        if (nsStr == null || elemStr == null)
            return new UnknownComponentError();

        var nsAndelemIdValidation = from ns in NameSpace.ValidNameSpace(nsStr)
                                    from elemId in ElementIdentifier.ValidElementIdentifier(elemStr)
                                    select (ns, elemId);

        return nsAndelemIdValidation.OnSuccess(
            tuple =>
            {
                var (ns, elemId) = tuple;

                if (!mdoc.IssuerSigned.IssuerNameSpaces.Value.TryGetValue(ns, out var items))
                    return new NamespaceNotFoundError(nsStr);

                var item = items.FirstOrDefault(i => i.ElementId.Equals(elemId));
                if (item == null)
                    return new ElementNotFoundError(nsStr, elemStr);

                return ClaimPathSelection.Create([item.Element.ToJToken()]);
            }
        );
    }
}
