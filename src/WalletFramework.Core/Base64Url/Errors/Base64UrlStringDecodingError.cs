using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Base64Url.Errors;

public record Base64UrlStringDecodingError(string Input, Exception E) : Error(
    $"The input {Input} could not be decoded with Base64Url", E);
