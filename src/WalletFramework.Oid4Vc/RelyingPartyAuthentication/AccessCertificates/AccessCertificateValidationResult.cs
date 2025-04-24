using LanguageExt;
using WalletFramework.Core.X509;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates;

public record AccessCertificateValidationResult
{
    public bool IsValid { get; }

    public Option<Exception> Exception { get; }

    private AccessCertificateValidationResult(bool isValid) => IsValid = isValid;

    private AccessCertificateValidationResult(Exception exception) : this(false) => Exception = exception;

    public static AccessCertificateValidationResult Validate(
        AccessCertificate accessCertificate,
        RpRegistrarCertificate rpRegistrarCertificate)
    {
        string authorityKeyId;
        try
        {
            authorityKeyId = accessCertificate.GetRoot().ToSystemX509Certificate().GetAuthorityKeyId()
                             ?? throw new InvalidOperationException("Could not get root certificate authority key ID");
        }
        catch (Exception e)
        {
            return new AccessCertificateValidationResult(e);
        }

        var isTrustChainValid = accessCertificate.TrustChain.IsTrustChainValid();
        if (isTrustChainValid)
        {
            try
            {
                var subjectKeyId =
                    rpRegistrarCertificate.AsX509Certificate().ToSystemX509Certificate().GetSubjectKeyId()
                    ?? throw new InvalidOperationException("Could not get rp registrar certificate subject key ID");

                var result = string.Equals(authorityKeyId, subjectKeyId, StringComparison.InvariantCultureIgnoreCase);
                return new AccessCertificateValidationResult(result);
            }
            catch (Exception e)
            {
                return new AccessCertificateValidationResult(e);
            }
        }
        else
        {
            return new AccessCertificateValidationResult(false);
        }
    }
}
