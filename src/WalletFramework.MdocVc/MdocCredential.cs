using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc.Display;

namespace WalletFramework.MdocVc;

public record MdocCredential(
    Mdoc Mdoc,
    CredentialId CredentialId,
    CredentialSetId CredentialSetId,
    Option<List<MdocDisplay>> Displays,
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
