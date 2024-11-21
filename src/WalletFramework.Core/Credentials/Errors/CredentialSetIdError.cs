using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Credentials.Errors;

public record CredentialSetIdError(string Value) : Error($"The CredentialSetId is not a valid GUID, value is: {Value}");
