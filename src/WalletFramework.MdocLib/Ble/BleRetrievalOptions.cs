using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Ble.BleUuids;
using WalletFramework.MdocLib.Ble.Errors;
using WalletFramework.MdocLib.Cbor;

namespace WalletFramework.MdocLib.Ble;

public record BleRetrievalOptions(
    bool PeripheralServerModeSupported,
    bool CentralClientModeSupported,
    Option<BleUuid> Server2ClientUuid,
    Option<BleUuid> Client2ServerUuid)
{
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewMap();

        var peripheralModeSupportedLabel = CBORObject.FromObject(0);
        result.Add(peripheralModeSupportedLabel, CBORObject.FromObject(PeripheralServerModeSupported));
        
        var centralModeSupportedLabel = CBORObject.FromObject(1);
        result.Add(centralModeSupportedLabel, CBORObject.FromObject(CentralClientModeSupported));

        Server2ClientUuid.IfSome(uuid =>
        {
            var serverUuidLabel = CBORObject.FromObject(10);
            result.Add(serverUuidLabel, CBORObject.FromObject(uuid));
        });
        
        Client2ServerUuid.IfSome(uuid =>
        {
            var clientUuidLabel = CBORObject.FromObject(11);
            result.Add(clientUuidLabel, CBORObject.FromObject(uuid));
        });
        
        return result;
    }

    public static Validation<BleRetrievalOptions> FromCbor(CBORObject input)
    {
        var peripheralServerModeSupported = input.GetByLabel(0).OnSuccess(serverModeSupportedCbor =>
        {
            return serverModeSupportedCbor.AsBoolean();
        });

        var centralClientModeSupported = input.GetByLabel(1).OnSuccess(clientModeSupportedCbor =>
        {
            return clientModeSupportedCbor.AsBoolean();
        });

        var server2ClientUuid = (
            from uuidCbor in input.GetByLabel(10)
            from uuid in BleUuid.FromCbor(uuidCbor, "server2Client")
            select uuid).ToOption();
        
        var client2ServerUuid = (
            from uuidCbor in input.GetByLabel(11)
            from uuid in BleUuid.FromCbor(uuidCbor, "client2Server")
            select uuid).ToOption();

        if (server2ClientUuid.IsNone && client2ServerUuid.IsNone)
            return new NoServiceUuidFoundError();

        return
            from serverModeSupported in peripheralServerModeSupported
            from clientModeSupported in centralClientModeSupported
            let s2cUuid = server2ClientUuid
            let c2sUuid = client2ServerUuid
            select new BleRetrievalOptions(
                serverModeSupported,
                clientModeSupported,
                s2cUuid,
                c2sUuid);
    }
}

public static class BleRetrievalOptionsFun
{
    public static BleRetrievalOptions BleRetrievalOptionForCentral => new(
        false,
        true,
        Option<BleUuid>.None, 
        BleUuidFun.CreateServiceUuid());

    public static BleRetrievalOptions BleRetrievalOptionsForPeripheral => new(
        true,
        false,
        BleUuidFun.CreateServiceUuid(),
        Option<BleUuid>.None);
}
