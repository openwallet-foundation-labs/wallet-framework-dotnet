namespace WalletFramework.Core.Functional.Errors;

public record ObjectRequirementsAreNotMetError<T>(string errorMessage) : Error($"Requirements of Type `{typeof(T).Name}` were not met: {errorMessage}");
