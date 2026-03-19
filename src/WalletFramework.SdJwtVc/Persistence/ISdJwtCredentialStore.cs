using LanguageExt;
using WalletFramework.Core.Credentials;

namespace WalletFramework.SdJwtVc.Persistence;

public interface ISdJwtCredentialStore
{
    Task<Unit> Save(SdJwtCredential credential);

    Task<Option<SdJwtCredential>> Get(CredentialId id);

    Task<IReadOnlyList<SdJwtCredential>> List();

    Task<Unit> Delete(CredentialId id);
}
