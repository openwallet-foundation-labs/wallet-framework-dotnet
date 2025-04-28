using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.Implementations;

public class IRpRegistrarService : Abstractions.IRpRegistrarService
{
    private const string HardcodedCertificate = @"
-----BEGIN CERTIFICATE-----
MIIBdTCCARugAwIBAgIUHsSmbGuWAVZVXjqoidqAVClGx4YwCgYIKoZIzj0EAwIw
GzEZMBcGA1UEAwwQR2VybWFuIFJlZ2lzdHJhcjAeFw0yNTAzMzAxOTU4NTFaFw0y
NjAzMzAxOTU4NTFaMBsxGTAXBgNVBAMMEEdlcm1hbiBSZWdpc3RyYXIwWTATBgcq
hkjOPQIBBggqhkjOPQMBBwNCAASQWCESFd0Ywm9sK87XxqxDP4wOAadEKgcZFVX7
npe3ALFkbjsXYZJsTGhVp0+B5ZtUao2NsyzJCKznPwTz2wJcoz0wOzAaBgNVHREE
EzARgg9mdW5rZS13YWxsZXQuZGUwHQYDVR0OBBYEFMxnKLkGifbTKrxbGXcFXK6R
FQd3MAoGCCqGSM49BAMCA0gAMEUCIQD4RiLJeuVDrEHSvkPiPfBvMxAXRC6PuExo
pUGCFdfNLQIgHGSa5u5ZqUtCrnMiaEageO71rjzBlov0YUH4+6ELioY=
-----END CERTIFICATE-----";

    public Task<RpRegistrarCertificate> FetchRpRegistrarCertificate()
    {
        var cert = LoadFromPem(HardcodedCertificate);
        return Task.FromResult(new RpRegistrarCertificate(cert));
    }

    private static X509Certificate LoadFromPem(string pemString)
    {
        using var reader = new StringReader(pemString);
        var pemReader = new PemReader(reader);
        return (X509Certificate)pemReader.ReadObject();
    }
}
