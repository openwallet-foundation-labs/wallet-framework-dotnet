using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Cose;
using static WalletFramework.MdocLib.Security.Cose.CoseKey;

namespace WalletFramework.MdocLib.Device;

public record DeviceKeyInfo(
    CoseKey CoseKey,
    Option<KeyAuthorizations> KeyAuthorizations,
    Option<KeyInfo> KeyInfo)
{
    public static Validation<DeviceKeyInfo> ValidDeviceKeyInfo(CBORObject deviceKeyInfo) =>
        from deviceKeyCbor in deviceKeyInfo.GetByLabel("deviceKey")
        from deviceKey in FromCbor(deviceKeyCbor)
        select new DeviceKeyInfo(
            // TODO: Implement KeyAuthorizations and KeyInfo
            deviceKey, Option<KeyAuthorizations>.None, Option<KeyInfo>.None);
}
