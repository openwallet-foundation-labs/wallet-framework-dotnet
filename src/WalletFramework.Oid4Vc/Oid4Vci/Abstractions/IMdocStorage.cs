using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocVc;

namespace WalletFramework.Oid4Vc.Oid4Vci.Abstractions;

public interface IMdocStorage
{
    public Task<Unit> Add(MdocRecord record);

    public Task<Option<MdocRecord>> Get(CredentialId credentialId);
    
    public Task<Option<IEnumerable<MdocRecord>>> List(
        Option<ISearchQuery> query,
        int count = 100,
        int skip = 0);
    
    public Task<Option<IEnumerable<MdocRecord>>> ListAll() => List(Option<ISearchQuery>.None);

    public Task<Unit> Update(MdocRecord record);
    
    public Task<Unit> Delete(MdocRecord record);
}
