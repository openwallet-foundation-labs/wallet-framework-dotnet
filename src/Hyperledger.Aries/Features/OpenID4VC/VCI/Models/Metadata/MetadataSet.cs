using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Models.Metadata
{
    /// <summary>
    ///    Represents the metadata set of an OIDC issuer and authorization server.
    /// </summary>
    public record MetadataSet
    {
        /// <summary>
        ///    Gets the metadata of the OIDC issuer.
        /// </summary>
        public OidIssuerMetadata IssuerMetadata { get; }
        
        /// <summary>
        ///   Gets the metadata of the OIDC authorization server.
        /// </summary>
        public AuthorizationServerMetadata AuthorizationServerMetadata { get; }
        
        /// <summary>
        ///   Creates a new instance of <see cref="MetadataSet"/>.
        /// </summary>
        /// <param name="issuerMetadata"></param>
        /// <param name="authorizationServerMetadata"></param>
        public MetadataSet(OidIssuerMetadata issuerMetadata, AuthorizationServerMetadata authorizationServerMetadata) =>
            (IssuerMetadata, AuthorizationServerMetadata) = (issuerMetadata, authorizationServerMetadata);
    }
}
