using LanguageExt;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vci.Abstractions;

namespace WalletFramework.Oid4Vci.Implementations;

public class MdocCandidateService(
    IMdocCredentialStore mdocCredentialStore) : IMdocCandidateService
{
    public async Task<Option<IEnumerable<MdocCredential>>> GetCandidates(DeviceRequest deviceRequest)
    {
        var first = deviceRequest.DocRequests.First();
        var docType = first.ItemsRequest.DocType;

        // TODO: refactor with search query and constraint with items
        var candidates = await mdocCredentialStore.ListByDocType(docType);
        return candidates.ToList();
    }
}
