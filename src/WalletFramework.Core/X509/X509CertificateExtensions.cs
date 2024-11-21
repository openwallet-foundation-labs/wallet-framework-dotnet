using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace WalletFramework.Core.X509;

/// <summary>
///     Extension methods for <see cref="Org.BouncyCastle.X509.X509Certificate" />.
/// </summary>
public static class X509CertificateExtensions
{
    /// <summary>
    ///     Validates the trust chain of the certificate.
    /// </summary>
    /// <param name="trustChain">The trust chain to validate.</param>
    /// <returns>True if the trust chain is valid, otherwise false.</returns>
    public static bool IsTrustChainValid(this List<X509Certificate> trustChain)
    {
        var leafCert = trustChain.First();

        var rootCerts = new HashSet(
            trustChain
                .Where(cert => cert.IsSelfSigned())
                .Select(cert => new TrustAnchor(cert, null)));

        var intermediateCerts = new HashSet(
            trustChain
                .Where(cert => !cert.IsSelfSigned())
                .Append(leafCert));

        var storeSelector = new X509CertStoreSelector { Certificate = leafCert };
        
        var builderParams = new PkixBuilderParameters(rootCerts, storeSelector)
        {
            //TODO: Check if CRLs (Certificate Revocation Lists) are valid
            IsRevocationEnabled = false
        };

        var store = X509StoreFactory.Create(
            "Certificate/Collection",
            new X509CollectionStoreParameters(intermediateCerts));
        builderParams.AddStore(store);

        // This throws if validation fails
        var path = new PkixCertPathBuilder().Build(builderParams).CertPath;
        new PkixCertPathValidator().Validate(path, builderParams);

        return true;
    }

    public static X509Certificate ToBouncyCastleX509Certificate(this X509Certificate2 cert)
    {
        var certParser = new X509CertificateParser();
        return certParser.ReadCertificate(cert.GetRawCertData());
    }

    public static bool IsSelfSigned(this X509Certificate certificate) => 
        certificate.IssuerDN.Equivalent(certificate.SubjectDN);
}
