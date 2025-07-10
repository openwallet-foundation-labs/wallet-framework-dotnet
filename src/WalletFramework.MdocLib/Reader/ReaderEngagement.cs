using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Encoding;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Ble.BleUuids;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Device.Errors;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Reader;

// TODO: OriginInfos for increased security
public record ReaderEngagement(
    EngagementSecurity EngagementSecurity,
    List<DeviceRetrievalMethod> DeviceRetrievalMethods) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewMap();

        // Version is 1.0 when both OriginInfos and Capabilities are not present otherwise 1.1
        var versionLabel = CBORObject.FromObject(0);
        result.Add(versionLabel, CBORObject.FromObject("1.0"));
        
        var securityLabel = CBORObject.FromObject(1);
        result.Add(securityLabel, EngagementSecurity.ToCbor());
        
        var deviceRetrievalMethodsLabel = CBORObject.FromObject(2);
        result.Add(deviceRetrievalMethodsLabel, DeviceRetrievalMethods.ToCbor());

        return result;
    }

    public static Validation<ReaderEngagement> FromEngagementUri(EngagementUri input)
    {
        var cbor = input.AsCbor;

        var validVersion = cbor.GetByLabel(0).OnSuccess(versionCbor =>
        {
            var versionStr = versionCbor.AsString();
            if (versionStr is "1.0")
            {
                return Unit.Default;
            }
            else
            {
                return new InvalidVersionError("1.0", versionStr);
            }
        });

        var valid =
            from securityCbor in cbor.GetByLabel(1)
            from security in EngagementSecurity.FromCbor(securityCbor)
            select security;
        
        var validDeviceRetrievalMethods = 
            from methodsCbor in cbor.GetByLabel(2)
            from methods in DeviceRetrievalMethodFun.FromCbor(methodsCbor)
            select methods;

        return
            from _ in validVersion
            from security in valid
            from deviceRetrievalMethods in validDeviceRetrievalMethods
            select new ReaderEngagement(security, deviceRetrievalMethods);
    }
}

public static class ReaderEngagementFun
{
    public static string ToQrCodeValue(this ReaderEngagement readerEngagement)
    {
        var engagementBytes = readerEngagement.ToCbor().EncodeToBytes();
        var base64UrlStr = Base64UrlString.CreateBase64UrlString(engagementBytes);
        return "mdoc:" + base64UrlStr;
    }

    public static BleUuid GetServiceUuid(this ReaderEngagement readerEngagement)
    {
        var retrievalMethod = readerEngagement
            .DeviceRetrievalMethods
            .Single(method => method.TargetedConnection == 2);

        return retrievalMethod.RetrievalOptions.Server2ClientUuid.UnwrapOrThrow(
            new InvalidOperationException("Server2ClientUuid must have a value when ReaderEngagement is used"));
    }

    public static Sha256Hash ToSha256Hash(this ReaderEngagement readerEngagement)
    {
        var bytes = readerEngagement.ToCbor().ToTaggedCborByteString().AsCbor.EncodeToBytes();
        return Sha256Hash.ComputeHash(bytes);
    }
}
