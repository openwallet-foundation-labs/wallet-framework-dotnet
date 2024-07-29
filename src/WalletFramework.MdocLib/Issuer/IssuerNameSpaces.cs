using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Constants;
using static WalletFramework.MdocLib.NameSpace;

namespace WalletFramework.MdocLib.Issuer;

public record IssuerNameSpaces
{
    public Dictionary<NameSpace, List<IssuerSignedItem>> Value { get; }

    public List<IssuerSignedItem> this[NameSpace key] => Value[key];
    
    public static implicit operator IssuerNameSpaces(Dictionary<NameSpace, IEnumerable<IssuerSignedItem>> value) =>
        new(value);

    private IssuerNameSpaces(Dictionary<NameSpace, IEnumerable<IssuerSignedItem>> value) =>
        Value = value.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());

    internal static Validation<IssuerNameSpaces> ValidNameSpaces(CBORObject issuerSigned) =>
        issuerSigned.GetByLabel(NameSpacesLabel).OnSuccess(nameSpaces =>
        {
            var validDict = nameSpaces
                .ToDictionary(ValidNameSpace, issuerSignedItems => issuerSignedItems
                    .Values
                    .Select(IssuerSignedItem.ValidIssuerSignedItem)
                    .TraverseAll(item => item));

            return
                from dict in validDict
                select new IssuerNameSpaces(dict);
        });
}

public static class NameSpacesFun
{
    public static CBORObject ToCbor(this IssuerNameSpaces issuerNameSpaces)
    {
        var cbor = CBORObject.NewMap();
        foreach (var (key, items) in issuerNameSpaces.Value)
        {
            var array = CBORObject.NewArray();
            foreach (var item in items)
            {
                array.Add(item.ByteString);
            }

            cbor[key.Value] = array;
        }

        return cbor;
    }
}
