using static WalletFramework.Oid4Vc.Oid4Vp.Models.ClientIdScheme.ClientIdSchemeValue;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models
{
    /// <summary>
    ///     The client ID scheme used to obtain and validate metadata of the verifier.
    /// </summary>
    public record ClientIdScheme
    {
        /// <summary>
        ///     The client ID scheme value.
        /// </summary>
        public enum ClientIdSchemeValue
        {
            /// <summary>
            ///     The X509 SAN DNS client ID scheme.
            /// </summary>
            X509SanDns,

            /// <summary>
            ///     The verifier attestation client ID scheme.
            /// </summary>
            VerifierAttestation,
            
            /// <summary>
            ///     The Redirect Uri scheme.
            /// </summary>
            RedirectUri
        }

        /// <summary>
        ///     The Verifier Attestation scheme.
        /// </summary>
        public const string VerifierAttestationScheme = "verifier_attestation";

        /// <summary>
        ///     The X509 SAN DNS scheme.
        /// </summary>
        public const string X509SanDnsScheme = "x509_san_dns";
        
        /// <summary>
        ///     The Redirect Uri scheme.
        /// </summary>
        public const string RedirectUriScheme = "redirect_uri";

        /// <summary>
        ///     The client ID scheme value.
        /// </summary>
        public ClientIdSchemeValue Value { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientIdScheme" /> class.
        /// </summary>
        private ClientIdScheme(ClientIdSchemeValue value) => Value = value;

        /// <summary>
        ///     Creates a client ID scheme from the specified input.
        /// </summary>
        /// <param name="input">The input to create the client ID scheme from.</param>
        /// <returns>The client ID scheme created from the input.</returns>
        /// <exception cref="InvalidOperationException">The client ID scheme is not supported.</exception>
        public static ClientIdScheme CreateClientIdScheme(string input) =>
            input switch
            {
                X509SanDnsScheme => new ClientIdScheme(X509SanDns),
                RedirectUriScheme => new ClientIdScheme(RedirectUri),
                VerifierAttestationScheme =>
                    throw new NotImplementedException("Verifier Attestation not yet implemented"),
                _ => throw new InvalidOperationException($"Client ID Scheme {input} is not supported")
            };

        /// <summary>
        ///     Implicitly converts the input to a client ID scheme.
        /// </summary>
        public static implicit operator ClientIdScheme(string input) => CreateClientIdScheme(input);
    }
}
