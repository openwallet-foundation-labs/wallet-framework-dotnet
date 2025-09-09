using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;
using WalletFramework.MdocLib.Device.Errors;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.MdocLib.Device;

public record DeviceEngagement(
    EngagementSecurity Security,
    List<DeviceRetrievalMethod> DeviceRetrievalMethods) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewMap();

        // Version is 1.0 when both OriginInfos and Capabilities are not present otherwise 1.1
        var versionLabel = CBORObject.FromObject(0);
        result.Add(versionLabel,CBORObject.FromObject("1.0"));
        
        var securityLabel = CBORObject.FromObject(1);
        result.Add(securityLabel, Security.ToCbor());
        
        var deviceRetrievalMethodsLabel = CBORObject.FromObject(2);
        result.Add(deviceRetrievalMethodsLabel, DeviceRetrievalMethods.ToCbor());

        return result;
    }
    
    public static Validation<DeviceEngagement> FromCbor(CBORObject cbor)
    {
        var validSecurity = 
            from securityCbor in cbor.GetByLabel(1)
            from security in EngagementSecurity.FromCbor(securityCbor)
            select security;
        
        var validMethods = 
            from methodsCbor in cbor.GetByLabel(2)
            from methods in DeviceRetrievalMethodFun.FromCbor(methodsCbor)
            select methods;

        return
            from security in validSecurity
            from methods in validMethods
            select new DeviceEngagement(security, methods);
    }

    public static Validation<DeviceEngagement> FromEngagementUri(EngagementUri input)
    {
        var cbor = input.AsCbor;

        var validVersion = cbor.GetByLabel(0).OnSuccess(versionCbor =>
        {
            var versionStr = versionCbor.AsString();
            if (versionStr is "1.0") return Unit.Default;

            return new InvalidVersionError("1.0", versionStr);
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
            select new DeviceEngagement(security, deviceRetrievalMethods);
    }
}
