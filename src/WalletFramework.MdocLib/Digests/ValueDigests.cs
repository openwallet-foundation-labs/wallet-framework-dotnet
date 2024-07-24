using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Digests.Digest;
using static WalletFramework.MdocLib.Digests.DigestId;
using static WalletFramework.MdocLib.NameSpace;

namespace WalletFramework.MdocLib.Digests;

public readonly struct ValueDigests
{
    private Dictionary<NameSpace, Dictionary<DigestId, Digest>> Value { get; }

    private ValueDigests(Dictionary<NameSpace, Dictionary<DigestId, Digest>> value) => Value = value;
    
    public Dictionary<DigestId, Digest> this[NameSpace key] => Value[key];
    
    public static Validation<ValueDigests> ValidValueDigests(CBORObject valueDigests)
    {
        var validDict = valueDigests.ToDictionary(
            ValidNameSpace,
            digestsMap => digestsMap.ToDictionary(ValidDigestId, ValidDigest));

        return
            from dict in validDict
            select new ValueDigests(dict);
    }
}
