using WalletFramework.SdJwtLib.Models;

namespace WalletFramework.SdJwtLib.Roles
{
    public interface IHolder
    {
        public SdJwtDoc ReceiveCredential(string issuedSdJwt, string? issuerJwk = null, string? validJwtIssuer = null);

        public PresentationFormat CreatePresentationFormat(string issuerSignedJwt, Disclosure[] disclosures);
    }
}
