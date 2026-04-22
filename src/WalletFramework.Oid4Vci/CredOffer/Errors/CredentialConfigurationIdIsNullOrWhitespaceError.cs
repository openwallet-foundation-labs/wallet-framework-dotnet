using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vci.CredOffer.Errors;

public record CredentialConfigurationIdIsNullOrWhitespaceError()
    : Error("The CredentialConfigurationId is null or whitespace");
