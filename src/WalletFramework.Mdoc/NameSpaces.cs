using PeterO.Cbor;
using WalletFramework.Functional;
using static WalletFramework.Mdoc.Common.Constants;
using static WalletFramework.Mdoc.NameSpace;

namespace WalletFramework.Mdoc;

public readonly struct NameSpaces
{
    public Dictionary<NameSpace, List<IssuerSignedItem>> Value { get; }

    public List<IssuerSignedItem> this[NameSpace key] => Value[key];
    
    public static implicit operator NameSpaces(Dictionary<NameSpace, IEnumerable<IssuerSignedItem>> value) =>
        new(value);

    private NameSpaces(Dictionary<NameSpace, IEnumerable<IssuerSignedItem>> value) =>
        Value = value.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());

    internal static Validation<NameSpaces> ValidNameSpaces(CBORObject issuerSigned) =>
        issuerSigned.GetByLabel(NameSpacesLabel).OnSuccess(nameSpaces =>
        {
            var validDict = nameSpaces
                .ToDictionary(ValidNameSpace, issuerSignedItems => issuerSignedItems
                    .Values
                    .Select(IssuerSignedItem.ValidIssuerSignedItem)
                    .Traverse(item => item)
                );

            return
                from dict in validDict
                select new NameSpaces(dict);
        });

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewMap();
        foreach (var (key, items) in Value)
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
