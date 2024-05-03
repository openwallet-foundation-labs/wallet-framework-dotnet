using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialOffer.GrantTypes
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
        
        /// <summary>
        ///     Specifying whether the user must send a User Pin along with the Token Request in a Pre-Authorized Code Flow.
        /// </summary>
        [Obsolete]
        [JsonProperty("user_pin_required")]
        public bool? UserPinRequired { get; set; }
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
        public int? Length { get; set; }
        
        /// <summary>
        ///     Gets or sets a description of the transaction code.
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
        
        /// <summary>
        ///    Gets or sets the input mode of the transaction code which specifies the valid character set. (Must be 'numeric' ot 'text')
        /// </summary>
        [JsonProperty("input_mode")]
        public string? InputMode { get; set; }
    }
}
