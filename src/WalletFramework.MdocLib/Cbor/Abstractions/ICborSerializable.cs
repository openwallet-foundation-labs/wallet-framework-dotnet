using PeterO.Cbor;

namespace WalletFramework.MdocLib.Cbor.Abstractions;

public interface ICborSerializable
{
     public CBORObject ToCbor();
}
