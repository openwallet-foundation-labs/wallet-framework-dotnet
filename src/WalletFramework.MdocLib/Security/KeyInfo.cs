using PeterO.Cbor;

namespace WalletFramework.MdocLib.Security;

public record KeyInfo(Dictionary<int, CBORObject> Value);
