using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Security;
using static WalletFramework.MdocLib.Device.DeviceKey;

namespace WalletFramework.MdocLib.Device;

public record DeviceKeyInfo(
    DeviceKey DeviceKey,
    Option<KeyAuthorizations> KeyAuthorizations,
    Option<KeyInfo> KeyInfo)
{
    public static Validation<DeviceKeyInfo> ValidDeviceKeyInfo(CBORObject deviceKeyInfo) =>
        from deviceKeyCbor in deviceKeyInfo.GetByLabel("deviceKey")
        from deviceKey in ValidDeviceKey(deviceKeyCbor)
        select new DeviceKeyInfo(
            // TODO: Implement KeyAuthorizations and KeyInfo
            deviceKey, Option<KeyAuthorizations>.None, Option<KeyInfo>.None);
}
