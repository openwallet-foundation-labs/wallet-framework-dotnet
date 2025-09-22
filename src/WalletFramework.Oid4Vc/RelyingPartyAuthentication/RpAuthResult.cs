using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates.AccessCertificate;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RpAuthResultFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication;

public record RpAuthResult
{
    public RpTrustLevel TrustLevel { get; }
    
    public Option<AccessCertificate> AccessCertificate { get; }

    private RpAuthResult(
        AccessCertificateValidationResult accessCertificateValidationResult,
        OverAskingValidationResult overAskingValidationResult,
        AccessCertificate accessCertificate) =>
        (TrustLevel, AccessCertificate) = (CalculateTrustLevel(accessCertificateValidationResult, overAskingValidationResult), accessCertificate);

    private RpAuthResult(RpTrustLevel trustLevel, Option<AccessCertificate> accessCertificate) => (TrustLevel, AccessCertificate) = (trustLevel, accessCertificate);

    public static RpAuthResult ValidateRequestObject(
        RequestObject requestObject,
        RpRegistrarCertificate rpRegistrarCertificate)
    {
        var accessCertificateValidation = FromRequestObject(requestObject);
        
        return accessCertificateValidation.Match(
            accessCertificate =>
            {
                var accessCertificateValidationResult = AccessCertificateValidationResult.Validate(accessCertificate, rpRegistrarCertificate);
                var overAskingValidationResult = OverAskingValidationResult.Validate(requestObject);
                return new RpAuthResult(accessCertificateValidationResult, overAskingValidationResult, accessCertificate);
            },
            _ => new RpAuthResult(RpTrustLevel.ValidationFailed, Option<AccessCertificate>.None)
        );
    }

    public static RpAuthResult GetWithLevelUnknown() => new(RpTrustLevel.Unknown, Option<AccessCertificate>.None);
}

public static class RpAuthResultFun
{
    public static RpTrustLevel CalculateTrustLevel(
        AccessCertificateValidationResult accessCertificateValidationResult,
        OverAskingValidationResult overAskingValidationResult)
    {
        if (accessCertificateValidationResult.IsValid is false)
        {
            return RpTrustLevel.AccessCertificateValidationFailed;
        }

        return overAskingValidationResult.IsValid
            ? RpTrustLevel.ValidationSuccessful
            : RpTrustLevel.OverAskingValidationFailed;
    }
}
