using Newtonsoft.Json;

namespace WalletFramework.Oid4Vci.AuthFlow.Models;

/// <summary>
///     Represents the client options that are used during the VCI Authorization Code Flow. Here the wallet acts as the
///     client.
/// </summary>
public record ClientOptions
{
    /// <summary>
    ///     Identifier of the client (wallet)
    /// </summary>
    public string ClientId { get; init; }

    /// <summary>
    ///     Redirect URI that the Authorization Server will use after the authorization was successful.
    /// </summary>
    public string RedirectUri { get; init; }

    /// <summary>
    ///     Identifier of the wallet issuer
    /// </summary>
    public string WalletIssuer { get; init; }

#pragma warning disable CS8618
    /// <summary>
    ///     Parameterless Default Constructor.
    /// </summary>
    public ClientOptions()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientOptions" /> class.
    /// </summary>
    /// <param name="clientId">Identifier of the client (wallet)</param>
    /// <param name="walletIssuer">Identifier of the wallet issuer</param>
    /// <param name="redirectUri">Redirect URI that the Authorization Server will use after the authorization was successful.</param>
    [JsonConstructor]
    public ClientOptions(string clientId, string walletIssuer, string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(walletIssuer)
            || !Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
        {
            throw new ArgumentException("Invalid Client Options");
        }

        ClientId = clientId;
        WalletIssuer = walletIssuer;
        RedirectUri = redirectUri;
    }
}
