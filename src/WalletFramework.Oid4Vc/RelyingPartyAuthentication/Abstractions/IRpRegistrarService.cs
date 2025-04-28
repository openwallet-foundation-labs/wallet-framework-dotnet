namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.Abstractions;

public interface IRpRegistrarService
{
    public Task<RpRegistrarCertificate> FetchRpRegistrarCertificate();
}
