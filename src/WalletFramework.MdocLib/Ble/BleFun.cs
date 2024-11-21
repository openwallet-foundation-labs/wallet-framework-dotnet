using static WalletFramework.MdocLib.Ble.BleFlags;

namespace WalletFramework.MdocLib.Ble;

public static class BleFun
{
    public static List<byte[]> ChunkBytes(byte[] data, int mtu)
    {
        mtu -= 3;

        var chunks = data
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / mtu)
            .Select(group => group.Select(x => x.value))
            .Select(bytes => bytes.Prepend(MoreIncomingFlag).ToArray())
            .ToList();

        chunks[^1][0] = LastChunkFlag;

        return chunks;
    }
}
