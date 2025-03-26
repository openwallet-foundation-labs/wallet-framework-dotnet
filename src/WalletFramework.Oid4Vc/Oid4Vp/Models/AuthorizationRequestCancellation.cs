using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record AuthorizationRequestCancellation(
    Option<Uri> ResponseUri,
    List<VpError> Errors);

public static class AuthorizationRequestCancellationFun
{
    public static Seq<Error> GetErrorSeq(this AuthorizationRequestCancellation cancellation)
    {
        var errors = cancellation.Errors.Select(
            error => error as Error ?? new InvalidRequestError("Could not parse the Authorization Request"));

        return errors.ToSeq();
    }
}
