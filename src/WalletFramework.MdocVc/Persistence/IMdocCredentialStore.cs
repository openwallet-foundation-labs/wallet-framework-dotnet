using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocLib;

namespace WalletFramework.MdocVc.Persistence;

public interface IMdocCredentialStore
{
    Task<Unit> Add(MdocCredential credential);

    Task<Option<MdocCredential>> Get(CredentialId id);

    Task<IReadOnlyList<MdocCredential>> List();

    Task<IReadOnlyList<MdocCredential>> ListByDocType(DocType docType);

    Task<Unit> Update(MdocCredential credential);

    Task<Unit> Delete(CredentialId id);
}
