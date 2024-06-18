using WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization;

public record AuthorizationData
{
    public ClientOptions ClientOptions { get; }
    
    public MetadataSet MetadataSet { get; }
    
    public string[] CredentialConfigurationIds { get; }
    
    public AuthorizationCode? AuthorizationCode { get; }
    
    public AuthorizationData(
        ClientOptions clientOptions,
        MetadataSet metadataSet,
        string[] credentialConfigurationIds,
        AuthorizationCode? authorizationCode)
    {
        ClientOptions = clientOptions;
        MetadataSet = metadataSet;
        CredentialConfigurationIds = credentialConfigurationIds;
        AuthorizationCode = authorizationCode;
    }
}
