using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Request.Errors;

public record DecryptionError<T>(Exception E) : Error($"Could not decrypt into {typeof(T).Name}", E);
