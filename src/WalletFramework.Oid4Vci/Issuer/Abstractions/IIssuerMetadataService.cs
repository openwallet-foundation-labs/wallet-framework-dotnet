using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vci.Issuer.Abstractions;

public interface IIssuerMetadataService
{
    public Task<Validation<IssuerMetadata>> ProcessMetadata(Uri issuerEndpoint, Locale language);
}
