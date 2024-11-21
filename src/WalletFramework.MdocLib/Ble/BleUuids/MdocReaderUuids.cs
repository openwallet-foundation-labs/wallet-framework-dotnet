using WalletFramework.Core.Functional;
using static WalletFramework.MdocLib.Ble.BleUuids.BleUuid;

namespace WalletFramework.MdocLib.Ble.BleUuids;

public static class MdocReaderUuids
{
    public static BleUuid State => FromString("00000005-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();

    public static BleUuid Client2Server => FromString("00000006-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();

    public static BleUuid Server2Client => FromString("00000007-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();

    public static BleUuid Ident => FromString("00000008-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();
}
