using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RpAuthResultFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication;

public record RpAuthResult
{
    public RpTrustLevel TrustLevel { get; }

    private RpAuthResult(
        AccessCertificateValidationResult accessCertificateValidationResult,
        OverAskingValidationResult overAskingValidationResult) =>
        TrustLevel = CalculateTrustLevel(accessCertificateValidationResult, overAskingValidationResult);

    private RpAuthResult(RpTrustLevel trustLevel) => TrustLevel = trustLevel;

    public static RpAuthResult ValidateRequestObject(
        RequestObject requestObject,
        RpRegistrarCertificate rpRegistrarCertificate)
    {
        var result =
            from accessCertificate in AccessCertificate.FromRequestObject(requestObject)
            let accessCertificateValidationResult = AccessCertificateValidationResult.Validate(accessCertificate, rpRegistrarCertificate)
            let overAskingValidationResult = OverAskingValidationResult.Validate(requestObject)
            select new RpAuthResult(accessCertificateValidationResult, overAskingValidationResult);

        return result.Match(
            rpAuthResult => rpAuthResult,
            // TODO: Log
            _ => new RpAuthResult(RpTrustLevel.Abort)
        );
    }

    public static RpAuthResult GetWithLevelAbort() => new(RpTrustLevel.Abort);
}

public static class RpAuthResultFun
{
    public static RpTrustLevel CalculateTrustLevel(
        AccessCertificateValidationResult accessCertificateValidationResult,
        OverAskingValidationResult overAskingValidationResult)
    {
        if (accessCertificateValidationResult.IsValid is false)
        {
            return RpTrustLevel.Abort;
        }

        return overAskingValidationResult.IsValid
            ? RpTrustLevel.Green
            : RpTrustLevel.Red;
    }
}
