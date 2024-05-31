
namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Authorization
{
    /// <summary>
    ///     Represents the parameters required for the authorization during the VCI authorization code flow.
    ///     The code itself will be part of the Client redirect uri that is created by the authorization server upon
    ///     successful authorization.
    /// </summary>
    public record AuthorizationCodeParameters(
        string Challenge,
        string Verifier)
    {
        /// <summary>
        ///     Gets the code challenge.
        /// </summary>
        public string Challenge { get; } = Challenge;

        /// <summary>
        ///    Gets the code challenge method. SHA-256 is the only supported method.
        /// </summary>
        public string CodeChallengeMethod => "S256";

        /// <summary>
        ///     Gets the code verifier.
        /// </summary>
        public string Verifier { get; } = Verifier;
    }
}
