using LanguageExt;
using Org.BouncyCastle.X509;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates.Errors;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.AccessCertificates;

public record AccessCertificate
{
    public List<X509Certificate> TrustChain { get; }

    private AccessCertificate(List<X509Certificate> certificates)
    {
        TrustChain = certificates;
    }

    public static Validation<AccessCertificate> FromRequestObject(RequestObject requestObject)
    {
        List<X509Certificate> certificates;
        try
        {
            certificates = requestObject.GetCertificates();
        }
        catch (Exception e)
        {
            return new AccessCertificateError("Could not get certificates", e);
        }

        if (certificates.Count < 1)
        {
            return new AccessCertificateError("No certificates found", Option<Exception>.None);
        }

        return new AccessCertificate(certificates);
    }
    
    public X509Certificate GetLeaf() => TrustChain[0];
    
    public X509Certificate GetRoot() => TrustChain.Last();
}
