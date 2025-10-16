using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Storage;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public class MdocCandidateService(
    IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> repository) : IMdocCandidateService
{
    public async Task<Option<IEnumerable<MdocCredential>>> GetCandidates(DeviceRequest deviceRequest)
    {
        var first = deviceRequest.DocRequests.First();
        var docType = first.ItemsRequest.DocType;

        // TODO: refactor with search query and constraint with items
        var mdocsOption = await repository.ListAll();
        var mdocs = mdocsOption.UnwrapOrThrow();
        var candidates = mdocs.Where(c => c.Mdoc.DocType == docType);
        return candidates.ToList();
    }
}
