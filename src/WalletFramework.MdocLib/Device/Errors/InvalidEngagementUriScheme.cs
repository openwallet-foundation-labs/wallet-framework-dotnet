using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Errors;

public record InvalidEngagementUriSchemeError(string Scheme, string Input) : Error(
    $"The input {Input} does not have the Scheme {Scheme}");
