using Newtonsoft.Json.Linq;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;

/// <summary>
///     Storage-only record for VCI AuthFlow sessions.
/// </summary>
public sealed record AuthFlowSessionRecord : RecordBase
{
    public string SessionState { get; init; }

    public string Serialized { get; init; }

#pragma warning disable CS8618
    // Used by EF
    // ReSharper disable once UnusedMember.Local
    private AuthFlowSessionRecord() : base(Guid.NewGuid())
    {
    }
#pragma warning restore CS8618

    public AuthFlowSessionRecord(AuthFlowSession domain) : base(Guid.NewGuid())
    {
        SessionState = domain.AuthFlowSessionState.AsString();
        Serialized = domain.EncodeToJson().ToString();
    }

    public AuthFlowSession ToDomain()
    {
        return AuthFlowSessionJson.DecodeFromJson(JObject.Parse(Serialized));
    }
}
