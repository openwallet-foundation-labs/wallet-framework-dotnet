#nullable enable

using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer.GrantTypes
{
    /// <summary>
    ///     Represents the parameters for the 'pre-authorized_code' grant type.
    /// </summary>
    public class PreAuthorizedCode
    {
        /// <summary>
        ///     Gets or sets the pre-authorized code representing the Credential Issuer's authorization for the Wallet to obtain
        ///     Credentials of a certain type.
        /// </summary>
        [JsonProperty("pre-authorized_code")]
        public string Value { get; set; } = null!;
        
        /// <summary>
        ///     Specifying whether the user must send a Transaction Code along with the Token Request in a Pre-Authorized Code Flow.
        /// </summary>
        [JsonProperty("tx_code")]
        public TransactionCode? TransactionCode { get; set; }
    }

    /// <summary>
    ///    Represents the details of the expected Transaction Code.
    /// </summary>
    public class TransactionCode
    {
        /// <summary>
        ///     Gets or sets the length of the transaction code.
        /// </summary>
        [JsonProperty("length")]
        public TransactionCode? Length { get; set; }
        
        /// <summary>
        ///     Gets or sets a description of the transaction code.
        /// </summary>
        [JsonProperty("description")]
        public TransactionCode? Description { get; set; }
        
        /// <summary>
        ///    Gets or sets the input mode of the transaction code which specifies the valid character set. (Must be 'numeric' ot 'text')
        /// </summary>
        [JsonProperty("input_mode")]
        public TransactionCode? InputMode { get; set; }
    }
}
