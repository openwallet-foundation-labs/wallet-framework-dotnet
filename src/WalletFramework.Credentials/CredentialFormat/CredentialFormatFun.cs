using WalletFramework.Core.Functional;

namespace WalletFramework.Credentials;

public static class CredentialFormatFun
{
    public static CredentialFormat CreateSdJwtVcFormat() =>
        CredentialFormat.ValidCredentialFormat(CredentialFormatConstants.SdJwtVcFormat).UnwrapOrThrow();

    public static CredentialFormat CreateSdJwtDcFormat() =>
        CredentialFormat.ValidCredentialFormat(CredentialFormatConstants.SdJwtDcFormat).UnwrapOrThrow();

    public static CredentialFormat CreateMdocFormat() =>
        CredentialFormat.ValidCredentialFormat(CredentialFormatConstants.MdocFormat).UnwrapOrThrow();
}
