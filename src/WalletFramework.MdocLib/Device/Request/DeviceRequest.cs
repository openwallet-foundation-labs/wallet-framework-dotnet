using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Versioning;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;

namespace WalletFramework.MdocLib.Device.Request;

public record DeviceRequest(Version Version, List<DocRequest> DocRequests) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("version", CBORObject.FromObject(Version.ToMajorMinorString()));

        var docRequestsArray = CBORObject.NewArray();
        foreach (var docRequest in DocRequests)
            docRequestsArray.Add(docRequest.ToCbor());

        cbor.Add("docRequests", docRequestsArray);

        return cbor;
    }

    public static Validation<DeviceRequest> FromCbor(CBORObject cbor)
    {
        var version = cbor.GetByLabel("version").OnSuccess(versionCbor =>
        {
            var versionString = versionCbor.AsString();
            return new Version(versionString);
        });

        var docRequests = cbor.GetByLabel("docRequests").OnSuccess(docRequestsCbor =>
        {
            try
            {
                return docRequestsCbor
                    .Values
                    .TraverseAll(DocRequest.FromCbor)
                    .Select(requests => requests.ToList());
            }
            catch (Exception e)
            {
                return new CborIsNotAMapOrAnArrayError(docRequestsCbor.ToString(), e);
            }
        });

        return
            from v in version
            from d in docRequests
            select new DeviceRequest(v, d);
    }
}

public static class DeviceRequestFun
{
    public static DeviceRequest CreatePidDeviceRequest()
    {
        var docTypeCbor = CBORObject.NewMap();
        docTypeCbor.Add(CBORObject.FromObject("docType"), CBORObject.FromObject("eu.europa.ec.eudi.pid.1"));
        
        var nameSpacesCbor = CBORObject.FromObject("eu.europa.ec.eudi.pid.1");
        var nameSpace = NameSpace.ValidNameSpace(nameSpacesCbor).UnwrapOrThrow();
        
        var familyNameId = DataElementIdentifier.ValidDataElementIdentifier("family_name").UnwrapOrThrow();
        var familyName = new DataElement(familyNameId, false);
        
        var countryId = DataElementIdentifier.ValidDataElementIdentifier("resident_country").UnwrapOrThrow();
        var country = new DataElement(countryId, false);
        
        var ageOver18Id = DataElementIdentifier.ValidDataElementIdentifier("age_over_18").UnwrapOrThrow();
        var ageOver18 = new DataElement(ageOver18Id, false);
        
        var dataElements = new Dictionary<NameSpace, List<DataElement>>
        {
            {
                nameSpace, 
                [
                    familyName,
                    country,
                    ageOver18
                ]
            }
        };
        
        var pidDocType = DocType.ValidDoctype(docTypeCbor).UnwrapOrThrow();
        
        var docRequest = new ItemsRequest(pidDocType, dataElements).ToDocRequest();

        return new DeviceRequest(new Version("1.0"), [docRequest]);
    }
}
