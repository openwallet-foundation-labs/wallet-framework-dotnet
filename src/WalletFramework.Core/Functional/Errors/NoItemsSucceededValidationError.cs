namespace WalletFramework.Core.Functional.Errors;

public record NoItemsSucceededValidationError<T>() : Error($"No Validations of Type `{typeof(T).Name}` were successful");
