using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Versioning;
using static WalletFramework.Core.Base64Url.Base64UrlString;

namespace WalletFramework.MdocLib.Device.Response;

public record DeviceResponse(
    Version Version,
    Option<List<Document>> Documents,
    Option<List<DocumentError>> DocumentErrors,
    StatusCode Status);

public static class DeviceResponseFun
{
    // TODO: Currently only supports one document
    public static DeviceResponse BuildDeviceResponse(this Document document)
    {
        // TODO: Error handling if no suited mdoc could be found
        var version = new Version("1.0");
        var documents = new List<Document> { document };
        var documentErrors = Option<List<DocumentError>>.None;
        var statusCode = StatusCode.Ok;

        return new DeviceResponse(version, documents, documentErrors, statusCode);
    }

    private static CBORObject ToCbor(this DeviceResponse deviceResponse)
    {
        var cbor = CBORObject.NewMap();

        cbor.Add("version", deviceResponse.Version.ToMajorMinorString());

        deviceResponse.Documents.IfSome(documents =>
        {
            var documentsArray = CBORObject.NewArray();
            foreach (var document in documents) 
                documentsArray.Add(document.ToCbor());
            
            cbor.Add("documents", documentsArray);
        });
        
        // TODO: Encode documentErrors, when its implemented
        
        var statusLabel = CBORObject.FromObject("status");
        var status = CBORObject.FromObject((int)deviceResponse.Status);
        cbor.Add(statusLabel, status);

        return cbor;
    }
    
    public static Base64UrlString EncodeToBase64Url(this DeviceResponse deviceResponse)
    {
        var bytes = deviceResponse.ToCbor().EncodeToBytes();
        return CreateBase64UrlString(bytes);
    }
}
