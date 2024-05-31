
namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Authorization
{
    /// <summary>
    /// Represents the client options that are used during the VCI Authorization Code Flow. Here the wallet acts as the client.
    /// </summary>
    public record ClientOptions
    {
        /// <summary>
        ///     Identifier of the client (wallet)
        /// </summary>
        public string ClientId { get; }
        
        /// <summary>
        ///     Identifier of the wallet issuer
        /// </summary>
        public string WalletIssuer { get; }
        
        /// <summary>
        ///     Redirect URI that the Authorization Server will use after the authorization was successful.
        /// </summary>
        public string RedirectUri { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOptions"/> class.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="walletIssuer"></param>
        /// <param name="redirectUri"></param>
        public ClientOptions(string clientId, string walletIssuer, string redirectUri)
        {
            ClientId = clientId;
            WalletIssuer = walletIssuer;
            RedirectUri = redirectUri;
        }
    }
}
