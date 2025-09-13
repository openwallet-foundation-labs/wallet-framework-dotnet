using WalletFramework.SdJwtLib.Models;

namespace WalletFramework.SdJwtVc;

public static class SdJwtCredentialExtensions
{
    public static SdJwtDoc ToSdJwtDoc(this SdJwtCredential record) =>
        new(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
}
