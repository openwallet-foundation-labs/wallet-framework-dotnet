using Newtonsoft.Json;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.Oid4Vp.Persistence;

/// <summary>
///     Storage-only model for OpenID4VP presentations.
/// </summary>
public sealed record CompletedPresentationRecord : RecordBase
{
    public string PresentationId { get; init; }

    public string ClientId { get; init; }

    public string Serialized { get; init; }

#pragma warning disable CS8618
    // ReSharper disable once UnusedMember.Local
    // Used by EF
    private CompletedPresentationRecord() : base(Guid.NewGuid())
    {
    }
#pragma warning restore CS8618

    public CompletedPresentationRecord(CompletedPresentation domain) : base(Guid.NewGuid())
    {
        PresentationId = domain.PresentationId;
        ClientId = domain.ClientId;
        Serialized = JsonConvert.SerializeObject(domain);
    }

    public CompletedPresentation ToDomainModel()
    {
        return JsonConvert.DeserializeObject<CompletedPresentation>(Serialized)!;
    }
}
