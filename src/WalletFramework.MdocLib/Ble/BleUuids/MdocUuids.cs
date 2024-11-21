using WalletFramework.Core.Functional;
using static WalletFramework.MdocLib.Ble.BleUuids.BleUuid;

namespace WalletFramework.MdocLib.Ble.BleUuids;

public static class MdocUuids
{
    public static BleUuid State => FromString("00000001-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();

    public static BleUuid Client2Server => FromString("00000002-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();

    public static BleUuid Server2Client => FromString("00000003-A123-48CE-896B-4C76973373E6").UnwrapOrThrow();
}
