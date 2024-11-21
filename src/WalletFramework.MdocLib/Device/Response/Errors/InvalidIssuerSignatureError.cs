using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Device.Response.Errors;

public record InvalidIssuerSignatureError() : Error("Verification of the Issuer Signature failed");
