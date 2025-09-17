using LanguageExt;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocVc;

namespace WalletFramework.Oid4Vc.Oid4Vci.Abstractions;

public interface IMdocCandidateService
{
    Task<Option<IEnumerable<MdocRecord>>> GetCandidates(DeviceRequest deviceRequest);
}
