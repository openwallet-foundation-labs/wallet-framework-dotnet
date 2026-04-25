using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vci.Issuer.Errors;

public record CredentialEndpointError(Exception E) : Error("The credential enpoint could not be processed", E);
