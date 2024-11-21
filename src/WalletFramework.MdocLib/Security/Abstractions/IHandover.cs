using PeterO.Cbor;

namespace WalletFramework.MdocLib.Security.Abstractions;

public interface IHandover
{
    public CBORObject ToCbor();

    public SessionTranscript ToSessionTranscript();
}
