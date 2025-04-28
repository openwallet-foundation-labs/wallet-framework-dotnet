using LanguageExt;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates.Errors;

public record AccessCertificateError(string Message, Option<Exception> Exception) : Error(Message, Exception);
