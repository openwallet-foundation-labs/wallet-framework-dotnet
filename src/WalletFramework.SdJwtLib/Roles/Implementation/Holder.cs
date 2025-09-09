using WalletFramework.SdJwtLib.Models;

namespace WalletFramework.SdJwtLib.Roles.Implementation
{
    public class Holder : IHolder
    {
        public PresentationFormat CreatePresentationFormat(string issuerSignedJwt, Disclosure[] disclosures)
        {
            var presentation = disclosures.Aggregate(issuerSignedJwt + "~", (current, disclosure) => current + $"{disclosure.Serialize()}~");
            return new PresentationFormat(presentation);
        }

        public SdJwtDoc ReceiveCredential(string issuedSdJwt, string? issuerJwk = null, string? validJwtIssuer = null)
        {
            SdJwtDoc doc = new SdJwtDoc(issuedSdJwt);
            
            if (!string.IsNullOrWhiteSpace(issuerJwk) && !string.IsNullOrWhiteSpace(validJwtIssuer))
                doc.AssertThatJwtSignatureIsValid(issuerJwk, validJwtIssuer);
            
            return new SdJwtDoc(issuedSdJwt);
        }
    }
}
