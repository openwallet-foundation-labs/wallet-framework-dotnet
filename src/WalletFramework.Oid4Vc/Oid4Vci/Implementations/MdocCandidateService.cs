using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public class MdocCandidateService(IMdocStorage mdocStorage) : IMdocCandidateService
{
    public async Task<Option<IEnumerable<MdocRecord>>> GetCandidates(DeviceRequest deviceRequest)
    {
        var first = deviceRequest.DocRequests.First();
        var docType = first.ItemsRequest.DocType;

        // TODO: refactor with search query and constraint with items
        var mdocsOption = await mdocStorage.ListAll();
        var mdocs = mdocsOption.UnwrapOrThrow();
        var candidates = mdocs.Where(record => record.DocType == docType);
        var result = Option<IEnumerable<MdocRecord>>.Some(candidates);
        return result;
    }
}
