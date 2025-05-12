namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication;

public enum RpTrustLevel
{
    ValidationSuccessful,
    AccessCertificateValidationFailed,
    OverAskingValidationFailed,
    ValidationFailed,
    Unknown
}
