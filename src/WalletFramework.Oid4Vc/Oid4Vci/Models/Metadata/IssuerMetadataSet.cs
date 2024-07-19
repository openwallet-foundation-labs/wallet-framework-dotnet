using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata;

/// <summary>
///    Represents the metadata set of an OIDC issuer and authorization server.
/// </summary>
public record IssuerMetadataSet
{
    /// <summary>
    ///    Gets the metadata of the OIDC issuer.
    /// </summary>
    public IssuerMetadata IssuerMetadata { get; }
        
    /// <summary>
    ///   Gets the metadata of the OIDC authorization server.
    /// </summary>
    public AuthorizationServerMetadata AuthorizationServerMetadata { get; }
        
    /// <summary>
    ///   Creates a new instance of <see cref="IssuerMetadataSet"/>.
    /// </summary>
    /// <param name="issuerMetadata"></param>
    /// <param name="authorizationServerMetadata"></param>
    public IssuerMetadataSet(IssuerMetadata issuerMetadata, AuthorizationServerMetadata authorizationServerMetadata)
    {
        IssuerMetadata = issuerMetadata;
        AuthorizationServerMetadata = authorizationServerMetadata;
    }
}
