using LanguageExt;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vp.RelyingPartyAuthentication.AccessCertificates.Errors;

public record AccessCertificateError(string Message, Option<Exception> Exception) : Error(Message, Exception);
