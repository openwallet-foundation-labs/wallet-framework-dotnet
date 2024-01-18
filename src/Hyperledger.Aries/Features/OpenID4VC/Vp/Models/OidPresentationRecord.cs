using System;
using Hyperledger.Aries.Storage;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     Used to persist OpenId4Vp presentations.
    /// </summary>
    public sealed class OidPresentationRecord : RecordBase
    {
        /// <summary>
        ///     Gets or sets the credentials the Holder presented to the Verifier.
        /// </summary>
        public PresentedCredential[] PresentedCredentials { get; set; }

        /// <summary>
        ///     Gets or sets the client id and identifies the Verifier.
        /// </summary>
        [JsonIgnore]
        public string ClientId
        {
            get => Get();
            set => Set(value, false);
        }

        /// <summary>
        ///     Gets or sets the type name.
        /// </summary>
        public override string TypeName => "AF.OidPresentationRecord";

        /// <summary>
        ///     Gets or sets the metadata of the Verifier.
        /// </summary>
        public string? ClientMetadata { get; set; }

        /// <summary>
        ///     Gets or sets the name of the presentation.
        /// </summary>
        [JsonIgnore]
        public string? Name
        {
            get => Get();
            set => Set(value!, false);
        }

#pragma warning disable CS8618
        /// <summary>
        ///     Parameterless Default Constructor.
        /// </summary>
        public OidPresentationRecord()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        ///     Constructor for Serialization.
        /// </summary>
        /// <param name="presentedCredentials">The credentials the Holder presented to the Verifier.</param>
        /// <param name="clientId">The client id for the Verifier.</param>
        /// <param name="id">The id of the record.</param>
        /// <param name="clientMetadata">The metadata of the Verifier.</param>
        /// <param name="name">The name of the presentation.</param>
        [JsonConstructor]
        private OidPresentationRecord(
            PresentedCredential[] presentedCredentials,
            string clientId,
            string id,
            string? clientMetadata,
            string? name)
        {
            ClientId = clientId;
            ClientMetadata = clientMetadata;
            Id = id;
            Name = name;
            PresentedCredentials = presentedCredentials;
        }
    }
}
