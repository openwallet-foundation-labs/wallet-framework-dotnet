using Org.BouncyCastle.X509;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication;

public record RpRegistrarCertificate
{
    private X509Certificate Value { get; }
    
    public RpRegistrarCertificate(X509Certificate certificate)
    {
        Value = certificate;
    }
    
    public X509Certificate AsX509Certificate() => Value;
}
