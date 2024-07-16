using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

public record AuthorizationData(
    ClientOptions ClientOptions,
    IssuerMetadata IssuerMetadata,
    AuthorizationServerMetadata AuthorizationServerMetadata,
    List<CredentialConfigurationId> CredentialConfigurationIds);
