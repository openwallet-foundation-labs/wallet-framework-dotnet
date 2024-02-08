using System;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     The client ID scheme used to obtain and validate metadata of the verifier.
    /// </summary>
    public abstract record ClientIdScheme
    {
        private const string VerifierAttestationScheme = "verifier_attestation";

        private const string X509SanDnsScheme = "x509_san_dns";

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientIdScheme" /> class.
        /// </summary>
        protected ClientIdScheme()
        {
        }

        /// <summary>
        ///     Creates a client ID scheme from the specified input.
        /// </summary>
        /// <param name="input">The input to create the client ID scheme from.</param>
        /// <returns>The client ID scheme created from the input.</returns>
        /// <exception cref="InvalidOperationException">The client ID scheme is not supported.</exception>
        public static ClientIdScheme CreateClientIdScheme(string input) =>
            input switch
            {
                X509SanDnsScheme => new X509SanDns(),
                VerifierAttestationScheme =>
                    throw new NotImplementedException("Verifier Attestation not yet implemented"),
                _ => throw new InvalidOperationException($"Client ID Scheme {input} is not supported")
            };

        /// <summary>
        ///     Implicitly converts the input to a client ID scheme.
        /// </summary>
        public static implicit operator ClientIdScheme(string input) => CreateClientIdScheme(input);

        /// <summary>
        ///     The X509 SAN DNS client ID scheme.
        /// </summary>
        public record X509SanDns : ClientIdScheme
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ClientIdScheme.X509SanDns" /> class.
            /// </summary>
            public X509SanDns()
            {
            }

            /// <summary>
            ///     Returns the X509 SAN DNS client ID scheme as a string.
            /// </summary>
            public override string ToString() => X509SanDnsScheme;
        }

        /// <summary>
        ///     The verifier attestation client ID scheme.
        /// </summary>
        public record VerifierAttestation : ClientIdScheme
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="ClientIdScheme.VerifierAttestation" /> class.
            /// </summary>
            public VerifierAttestation()
            {
            }

            /// <summary>
            ///     Returns the verifier attestation client ID scheme as a string.
            /// </summary>
            public override string ToString() => VerifierAttestationScheme;
        }
    }
}
