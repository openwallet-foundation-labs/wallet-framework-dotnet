using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;

namespace WalletFramework.MdocLib.Device.Request;

public record ItemsRequest(
    DocType DocType,
    Dictionary<NameSpace, List<DataElement>> Namespaces) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("docType", DocType.ToCbor());

        var namespacesCbor = CBORObject.NewMap();
        foreach (var (nameSpace, dataElements) in Namespaces)
        {
            var dataElementsCbor = CBORObject.NewMap();
            foreach (var (elementIdentifier, intentToRetain) in dataElements)
            {
                var intentToRetainCbor = CBORObject.FromObject(intentToRetain);
                dataElementsCbor.Add(elementIdentifier.ToString(), intentToRetainCbor);
            }

            namespacesCbor.Add(nameSpace.ToString(), dataElementsCbor);
        }

        cbor.Add("nameSpaces", namespacesCbor);

        return cbor;
    }

    public static Validation<ItemsRequest> FromCbor(CBORObject cbor)
    {
        var validDocType = 
            from docType in DocType.ValidDoctype(cbor)
            select docType;

        var validNameSpaces = cbor.GetByLabel("nameSpaces").OnSuccess(nameSpacesCbor =>
        {
            try
            {
                var xxx = nameSpacesCbor.Entries.TraverseAll(pair =>
                {
                    var nameSpace = pair.Key;
                    var n = NameSpace.ValidNameSpace(nameSpace);
                    var dataElements = pair.Value.Entries;
                    var de = dataElements.TraverseAll(x =>
                    {
                        var i = DataElementIdentifier
                            .ValidDataElementIdentifier(x.Key.AsString());
                        var itr = x.Value.AsBoolean();
                        return
                            from ii in i
                            select new DataElement(ii, itr);
                    }).Select(elements => elements.ToList());

                    return
                        from aa in n
                        from bb in de
                        select (NameSpace: aa, dataElements: bb);
                }).Select(tuples => tuples.ToDictionary(
                    tuple => tuple.NameSpace,
                    tuple => tuple.dataElements
                ));

                return xxx;
            }
            catch (Exception e)
            {
                return new CborIsNotAMapOrAnArrayError(nameSpacesCbor.ToString(), e).ToInvalid<Dictionary<NameSpace, List<DataElement>>>();
            }
        });

        return
            from docType in validDocType
            from nameSpaces in validNameSpaces
            select new ItemsRequest(docType, nameSpaces);
    }
}
