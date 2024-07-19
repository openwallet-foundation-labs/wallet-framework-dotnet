using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Credentials.Errors;

public record CredentialIdError(string Value) : Error($"The CredentialId is not a valid GUID, value is: {Value}");
