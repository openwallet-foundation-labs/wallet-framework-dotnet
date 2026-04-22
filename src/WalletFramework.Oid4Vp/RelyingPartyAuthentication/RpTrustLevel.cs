namespace WalletFramework.Oid4Vp.RelyingPartyAuthentication;

public enum RpTrustLevel
{
    ValidationSuccessful,
    AccessCertificateValidationFailed,
    OverAskingValidationFailed,
    ValidationFailed,
    Unknown
}
