using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

public record BatchSizeIsNotAPositiveNumberError() : Error("The value of `batch_size` is not a positive number");
