using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vci.Issuer.Errors;

public record CredentialIssuerIdError(string Value, Exception E) : Error($"The CredentialIssuerId could not be parsed. Value is {Value}", E);
