using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
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
    public static string? GetAuthorityKeyId(this X509Certificate2 cert)
    {
        var authorityKeyIdentifier = cert.Extensions["2.5.29.35"];
        if (authorityKeyIdentifier == null)
            return null;

        var asn1Object = new Asn1InputStream(authorityKeyIdentifier.RawData).ReadObject() as DerSequence;
        var derTaggedObject = asn1Object?[0] as DerTaggedObject;
        var hex = derTaggedObject?.GetObject().ToString().Trim('#');
        return hex;
    }
    
    public static string? GetSubjectKeyId(this X509Certificate2 cert)
    {
        const string subjectKeyIdentifierOid = "2.5.29.14";

        var ext = cert.Extensions[subjectKeyIdentifierOid] as X509SubjectKeyIdentifierExtension;
        return ext?.SubjectKeyIdentifier;
    }
    
    public static bool IsSelfSigned(this X509Certificate certificate) =>
        certificate.IssuerDN.Equivalent(certificate.SubjectDN);

    /// <summary>
    ///     Validates the trust chain of the certificate.
    /// </summary>
    /// <param name="trustChain">The trust chain to validate.</param>
    /// <returns>True if the trust chain is valid, otherwise false.</returns>
    public static bool IsTrustChainValid(this IEnumerable<X509Certificate> trustChain)
    {
        var chain = trustChain.ToList();
        if (chain.Count == 1)
        {
            return true;
        }

        var leafCert = chain.First();

        var subjects = chain.Select(cert => cert.SubjectDN);
        var rootCerts = new HashSet(
            chain
                .Where(cert => cert.IsSelfSigned() || !subjects.Contains(cert.IssuerDN))
                .Select(cert => new TrustAnchor(cert, null)));

        var intermediateCerts = new HashSet(
            chain
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

    public static X509Certificate2 ToSystemX509Certificate(this X509Certificate cert) =>
        new(cert.GetEncoded());
}
