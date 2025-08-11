using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.StatusList;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.CredentialSet.Persistence;

public sealed record CredentialDataSetRecord : RecordBase
{
    public string? SdJwtCredentialType { get; init; }

    public string? MDocCredentialType { get; init; }

    public string AttributesJson { get; init; }

    public string State { get; init; }

    public DateTime? NotBefore { get; init; }

    public DateTime? IssuedAt { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public string? StatusListJson { get; init; }

    public DateTime? RevokedAt { get; init; }

    public DateTime? DeletedAt { get; init; }

    public string IssuerId { get; init; }

    // Used by EF
#pragma warning disable CS8618
    // ReSharper disable once UnusedMember.Local
    private CredentialDataSetRecord() : base(Guid.NewGuid())
    {
    }
#pragma warning restore CS8618

    public CredentialDataSetRecord(CredentialDataSet domain) : base(Guid.Parse(domain.CredentialSetId.AsString()))
    {
        SdJwtCredentialType =
            (from v in domain.SdJwtCredentialType
            select v.ToString()).ToNullable();

        MDocCredentialType =
            (from d in domain.MDocCredentialType
            select d.ToString()).ToNullable();

        AttributesJson = JsonConvert.SerializeObject(domain.CredentialAttributes);
        State = domain.State.ToString();
        NotBefore = domain.NotBefore.ToNullable();
        IssuedAt = domain.IssuedAt.ToNullable();
        ExpiresAt = domain.ExpiresAt.ToNullable();

        StatusListJson =
            (from x in domain.StatusListEntry
            select JsonConvert.SerializeObject(x)).ToNullable();

        RevokedAt = domain.RevokedAt.ToNullable();
        DeletedAt = domain.DeletedAt.ToNullable();
        IssuerId = domain.IssuerId;
    }

    public CredentialDataSet ToDomain()
    {
        var setId = CredentialSetId.ValidCredentialSetId(RecordId.ToString()).UnwrapOrThrow();

        var sdJwtType = string.IsNullOrWhiteSpace(SdJwtCredentialType)
            ? Option<Vct>.None
            : Vct.ValidVct(SdJwtCredentialType!).ToOption();

        var mdocType = string.IsNullOrWhiteSpace(MDocCredentialType)
            ? Option<DocType>.None
            : DocType.ValidDoctype(new JValue(MDocCredentialType!)).ToOption();

        var attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(AttributesJson)
                         ?? new Dictionary<string, string>();

        var state = Enum.Parse<CredentialState>(State);

        var statusList = string.IsNullOrWhiteSpace(StatusListJson)
            ? Option<StatusListEntry>.None
            : StatusListEntry.FromJObject(JObject.Parse(StatusListJson!)).ToOption();

        return new CredentialDataSet(
            setId,
            sdJwtType,
            mdocType,
            attributes,
            state,
            statusList,
            ExpiresAt.ToOption(),
            IssuedAt.ToOption(),
            NotBefore.ToOption(),
            RevokedAt.ToOption(),
            DeletedAt.ToOption(),
            IssuerId);
    }
}
