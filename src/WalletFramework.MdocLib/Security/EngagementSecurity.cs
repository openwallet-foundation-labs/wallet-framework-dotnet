using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;
using WalletFramework.MdocLib.Security.Cose;

namespace WalletFramework.MdocLib.Security;

public record EngagementSecurity(
    CoseEllipticCurves CipherSuite,
    CoseKey SenderKey) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewArray();

        result.Add(CipherSuite.ToCbor());

        var keyBytes = SenderKey.ToCbor().EncodeToBytes();
        result.Add(keyBytes);

        return result;
    }

    public static Validation<EngagementSecurity> FromCbor(CBORObject input)
    {
        var validCipherSuite = input.GetByIndex(0).OnSuccess(CoseEllipticCurvesFun.FromCbor);
        var validSenderKey = input.GetByIndex(1).OnSuccess(CoseKey.FromCborBytes);

        return
            from cipherSuite in validCipherSuite
            from senderKey in validSenderKey
            select new EngagementSecurity(cipherSuite, senderKey);
    }
}
