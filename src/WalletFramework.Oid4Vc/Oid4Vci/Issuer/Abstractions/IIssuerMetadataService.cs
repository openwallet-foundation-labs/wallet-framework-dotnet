using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;

public interface IIssuerMetadataService
{
    public Task<Validation<IssuerMetadata>> ProcessMetadata(Uri issuerEndpoint, Locale language);
}
