namespace WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;

public interface IRpRegistrarService
{
    public Task<RpRegistrarCertificate> FetchRpRegistrarCertificate();
}
