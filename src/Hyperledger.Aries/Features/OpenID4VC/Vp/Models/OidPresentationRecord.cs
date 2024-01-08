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
        public string ClientId { get; }

        /// <summary>
        ///     Gets or sets the type name.
        /// </summary>
        public override string TypeName => "AF.OidPresentationRecord";

        /// <summary>
        ///     Gets or sets the metadata of the Verifier.
        /// </summary>
        public string? ClientMetadata { get; }

        /// <summary>
        ///     Gets or sets the name of the presentation.
        /// </summary>
        public string? Name { get; }

#pragma warning disable CS8618
        /// <summary>
        ///     This constructor is required for the record service but should not actually be used.
        /// </summary>
        public OidPresentationRecord()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        ///     Initializes a new instance of the <see cref="OidPresentationRecord" /> class.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientMetadata"></param>
        /// <param name="name"></param>
        /// <param name="presentedCredentials"></param>
        [JsonConstructor]
        public OidPresentationRecord(
            string clientId,
            string? clientMetadata,
            string? name,
            PresentedCredential[] presentedCredentials)
        {
            ClientId = clientId;
            ClientMetadata = clientMetadata;
            CreatedAtUtc = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
            Name = name;
            PresentedCredentials = presentedCredentials;
            RecordVersion = 1;
        }
    }
}
