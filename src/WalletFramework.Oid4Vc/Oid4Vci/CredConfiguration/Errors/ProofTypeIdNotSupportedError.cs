using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

public record ProofTypeIdNotSupportedError(string Value) : Error($"The ProofTypeId: `{Value}` is not supported");
