using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Ble.Errors;

public record CouldNotParseProximityMessageError(CBORObject Cbor) : Error(
    $"The CBOR object could not be parsed into DeviceEngagement or DeviceResponse. CBOR is {Cbor}");
