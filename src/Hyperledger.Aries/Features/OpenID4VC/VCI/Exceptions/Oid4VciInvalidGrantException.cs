using System;

namespace Hyperledger.Aries.Features.OpenID4VC.VCI.Exceptions
{
    /// <summary>
    ///     Represents an exception thrown when the grant within the Oid4Vci flow is invalid (e.g. wrong tx_code).
    /// </summary>
    public class Oid4VciInvalidGrantException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Oid4VciInvalidGrantException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public Oid4VciInvalidGrantException(string message)
            : base(message)
        {
        }
    }
}
