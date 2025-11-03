using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib;

namespace WalletFramework.MdocVc;

public record MdocCredential(
    Mdoc Mdoc,
    CredentialId CredentialId,
    CredentialSetId CredentialSetId,
    KeyId KeyId,
    CredentialState CredentialState,
    bool OneTimeUse,
    Option<DateTime> ExpiresAt) : ICredential
{
    public CredentialId GetId()
    {
        return CredentialId;
    }

    public CredentialSetId GetCredentialSetId()
    {
        return CredentialSetId;
    }
}
