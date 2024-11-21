using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Ble.Errors;

public record NoServiceUuidFoundError() : Error(
    "No UUID for service discovery was found");
