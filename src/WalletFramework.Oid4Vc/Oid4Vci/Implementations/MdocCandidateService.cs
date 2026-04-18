using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.MdocVc.Persistence;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

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
