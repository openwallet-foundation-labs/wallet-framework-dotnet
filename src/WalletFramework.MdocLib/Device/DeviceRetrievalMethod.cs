using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Ble;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;
using WalletFramework.MdocLib.Device.Errors;

namespace WalletFramework.MdocLib.Device;

public record DeviceRetrievalMethod(BleRetrievalOptions RetrievalOptions) : ICborSerializable
{
    public const uint BleTargetedConnection = 2;
    
    public uint TargetedConnection { get; } = BleTargetedConnection;

    public uint Version { get; } = 1;
    
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewArray();

        result.Add(CBORObject.FromObject(TargetedConnection));
        result.Add(CBORObject.FromObject(Version));
        result.Add(RetrievalOptions.ToCbor());

        return result;
    }

    public static Validation<DeviceRetrievalMethod> FromCbor(CBORObject input)
    {
        var targetedConnection = input.GetByIndex(0).OnSuccess(cbor =>
        {
            var targetedConnectionInt = cbor.AsInt32();
            if (targetedConnectionInt is 2)
            {
                return Unit.Default;
            }
            else
            {
                return new InvalidTargetedConnectionError((int)BleTargetedConnection, targetedConnectionInt);
            }
        });

        var version = input.GetByIndex(1).OnSuccess(cbor =>
        {
            int versionInt;
            try
            {
                versionInt = cbor.AsInt32();
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError("version", e);
            }
            
            if (versionInt is 1)
            {
                return Unit.Default;
            }
            else
            {
                return new InvalidVersionError("1.0", versionInt.ToString());
            }
        });

        var retrievalOptions = input
            .GetByIndex(2)
            .OnSuccess(BleRetrievalOptions.FromCbor);

        return
            from _ in targetedConnection
            from __ in version
            from options in retrievalOptions
            select new DeviceRetrievalMethod(options);
    }
}

public static class DeviceRetrievalMethodFun
{
    public static CBORObject ToCbor(this IEnumerable<DeviceRetrievalMethod> methods)
    {
        var result = CBORObject.NewArray();
        
        foreach (var deviceRetrievalMethod in methods)
        {
            result.Add(deviceRetrievalMethod.ToCbor());
        }

        return result;
    }

    public static Validation<List<DeviceRetrievalMethod>> FromCbor(CBORObject input)
    {
        try
        {
            var validMethods = input.Values.TraverseAll(DeviceRetrievalMethod.FromCbor);
            return
                from methods in validMethods
                select methods.ToList();
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(input.ToString(), e);
        }
    }
}
