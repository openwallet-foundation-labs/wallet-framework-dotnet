using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.Storage.Records;

namespace WalletFramework.SdJwtVc.Persistence;

public record SdJwtCredentialRecord : RecordBase
{
    public string EncodedIssuerSignedJwt { get; init; }

    public string CredentialSetId { get; init; }

    public string? KeyId { get; init; }

    public string CredentialState { get; init; }

    public bool OneTimeUse { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public string Vct { get; init; }

    public string? DisplaysJson { get; init; }

    public string? ClaimsJson { get; init; }

    public string? DisclosuresJson { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // ReSharper disable once UnusedMember.Local
    private SdJwtCredentialRecord() : base(Guid.NewGuid())
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public SdJwtCredentialRecord(SdJwtCredential credential) : base(Guid.Parse(credential.CredentialId.AsString()))
    {
        EncodedIssuerSignedJwt = credential.EncodedIssuerSignedJwt;
        CredentialSetId = credential.CredentialSetId.AsString();
        KeyId = credential.KeyId.ToNullable()?.AsString();
        Vct = credential.Vct.ToString();
        CredentialState = credential.CredentialState.ToString();
        OneTimeUse = credential.OneTimeUse;
        ExpiresAt = credential.ExpiresAt.MatchUnsafe(
            x => x,
            () => (DateTime?)null);
        DisplaysJson = credential.Displays.MatchUnsafe(
            JsonConvert.SerializeObject,
            () => null);
        ClaimsJson = JsonConvert.SerializeObject(credential.Claims);
        DisclosuresJson = JsonConvert.SerializeObject(credential.Disclosures);
    }

    public SdJwtCredential ToDomainModel()
    {
        var credentialId = CredentialId.ValidCredentialId(RecordId.ToString()).UnwrapOrThrow();
        var setId = Core.Credentials.CredentialSetId.ValidCredentialSetId(CredentialSetId).UnwrapOrThrow();
        var keyId = Core.Cryptography.Models.KeyId.ValidKeyId(KeyId).UnwrapOrThrow();
        var state = Enum.Parse<CredentialState>(CredentialState);
        var expires = ExpiresAt is null ? Option<DateTime>.None : Option<DateTime>.Some(ExpiresAt.Value);

        var displaysOption = string.IsNullOrWhiteSpace(DisplaysJson)
            ? Option<List<SdJwtDisplay>>.None
            : JsonConvert.DeserializeObject<List<SdJwtDisplay>>(DisplaysJson!);

        var disclosures = string.IsNullOrWhiteSpace(DisclosuresJson)
            ? []
            : JsonConvert.DeserializeObject<List<string>>(DisclosuresJson!) ?? new List<string>();

        var serializedSdJwtWithDisclosures = EncodedIssuerSignedJwt
                                             + (disclosures.Count > 0 ? "~" + string.Join("~", disclosures) + "~" : "~");

        var sdJwtDoc = new SdJwtDoc(serializedSdJwtWithDisclosures);

        return new SdJwtCredential(
            sdJwtDoc,
            credentialId,
            setId,
            displaysOption,
            keyId,
            state,
            OneTimeUse,
            expires);
    }
}
