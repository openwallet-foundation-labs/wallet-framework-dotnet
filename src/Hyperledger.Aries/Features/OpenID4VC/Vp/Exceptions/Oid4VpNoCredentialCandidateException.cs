using System;

namespace Hyperledger.Aries.Features.OpenID4VC.Vp.Exceptions
{
    /// <summary>
    ///     Represents an exception thrown when no credential candidate is found that can be used to answer an
    ///     Authorization Request during the Oid4Vp flow.
    /// </summary>
    public class Oid4VpNoCredentialCandidateException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VpNoCredentialCandidateException"/> class.
        /// </summary>
        public Oid4VpNoCredentialCandidateException()
            : base("No suitable credential candidates found")
        {
        }
    }
}
