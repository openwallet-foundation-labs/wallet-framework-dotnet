using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace WalletFramework.Oid4Vc.Oid4Vp.Extensions;

/// <summary>
///     Extension methods for <see cref="X509Certificate" />.
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

        var rootCerts =
            new HashSet(
                trustChain
                    .Skip(1)
                    .Where(cert => cert.IssuerDN.Equivalent(cert.SubjectDN))
                    .Select(cert => new TrustAnchor(cert, null))
                    .ToList()
            );

        var intermediateCerts =
            new HashSet(
                trustChain
                    .Skip(1)
                    .Where(cert => !cert.IssuerDN.Equivalent(cert.SubjectDN))
                    .Append(leafCert)
            );

        var builderParams =
            new PkixBuilderParameters(
                rootCerts,
                new X509CertStoreSelector
                {
                    Certificate = leafCert
                }
            )
            {
                //TODO: Check if CRLs (Certificate Revocation Lists) are valid
                IsRevocationEnabled = false
            };

        builderParams.AddStore(
            X509StoreFactory.Create(
                "Certificate/Collection",
                new X509CollectionStoreParameters(intermediateCerts)
            )
        );

        // This throws if validation fails
        new PkixCertPathValidator()
            .Validate(
                new PkixCertPathBuilder()
                    .Build(builderParams)
                    .CertPath,
                builderParams
            );

        return true;
    }
}
