using WalletFramework.Core.Functional;
using WalletFramework.MdocVc.Serialization;
using WalletFramework.Storage.Records;

namespace WalletFramework.MdocVc.Persistence;

/// <summary>
///     Storage-only model for mdoc credentials.
/// </summary>
public sealed record MdocCredentialRecord : RecordBase
{
    public string DocType { get; init; }

    public string Serialized { get; init; }

    // Used by EF
    // ReSharper disable once UnusedMember.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private MdocCredentialRecord() : base(Guid.NewGuid())
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public MdocCredentialRecord(MdocCredential mdoc) : base(Guid.Parse(mdoc.CredentialId.AsString()))
    {
        DocType = mdoc.Mdoc.DocType.AsString();
        Serialized = MdocCredentialSerializer.Serialize(mdoc);
    }

    public MdocCredential ToDomainModel()
    {
        return MdocCredentialSerializer.Deserialize(Serialized).UnwrapOrThrow();
    }
}
