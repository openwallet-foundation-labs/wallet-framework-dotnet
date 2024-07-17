using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Records;

/// <summary>
///   Represents the authorization session record. Used during the VCI Authorization Code Flow to hold session relevant information.
/// </summary>
public sealed class AuthFlowSessionRecord : RecordBase
{
    /// <summary>
    ///     The session specific id.
    /// </summary>
    [JsonIgnore]
    public VciSessionId SessionId
    {
        get => VciSessionId
            .ValidSessionId(Id)
            .UnwrapOrThrow(new InvalidOperationException("SessionId is corrupt"));
        set
        {
            string str = value;
            Id = str;
        }
    }

    /// <summary>
    ///     The authorization data.
    /// </summary>
    public AuthorizationData AuthorizationData { get; }
        
    /// <summary>
    ///     The parameters for the 'authorization_code' grant type.
    /// </summary>
    public AuthorizationCodeParameters AuthorizationCodeParameters { get; }

    /// <summary>
    ///    Initializes a new instance of the <see cref="AuthFlowSessionRecord" /> class.
    /// </summary>
    public override string TypeName  => "AF.VciAuthorizationSessionRecord";
        
#pragma warning disable CS8618
    /// <summary>
    ///   Initializes a new instance of the <see cref="AuthFlowSessionRecord" /> class.
    /// </summary>
    public AuthFlowSessionRecord()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    ///   Initializes a new instance of the <see cref="AuthFlowSessionRecord" /> class.
    /// </summary>
    /// <param name="authorizationData"></param>
    /// <param name="authorizationCodeParameters"></param>
    /// <param name="sessionId"></param>
    public AuthFlowSessionRecord(
        AuthorizationData authorizationData,
        AuthorizationCodeParameters authorizationCodeParameters,
        VciSessionId sessionId)
    {
        SessionId = sessionId;
        RecordVersion = 1;
        AuthorizationCodeParameters = authorizationCodeParameters;
        AuthorizationData = authorizationData;
    }
}

public static class AuthFlowSessionRecordFun
{
    private const string AuthorizationDataJsonKey = "authorization_data";
    private const string AuthorizationCodeParametersJsonKey = "authorization_code_parameters";

    public static JObject EncodeToJson(this AuthFlowSessionRecord record)
    {
        var authorizationData = record.AuthorizationData.EncodeToJson();
        var authorizationCodeParameters = JObject.FromObject(record.AuthorizationCodeParameters);

        return new JObject
        {
            { nameof(RecordBase.Id), record.Id },
            { AuthorizationDataJsonKey, authorizationData },
            { AuthorizationCodeParametersJsonKey, authorizationCodeParameters }
        };
    }

    public static AuthFlowSessionRecord DecodeFromJson(JObject json)
    {
        var idJson = json[nameof(RecordBase.Id)]!.ToObject<JValue>()!;
        var id = VciSessionIdFun.DecodeFromJson(idJson);

        var authCodeParameters = JsonConvert.DeserializeObject<AuthorizationCodeParameters>(
            json[AuthorizationCodeParametersJsonKey]!.ToString()
        );

        var authorizationData = AuthorizationDataFun
            .DecodeFromJson(json[AuthorizationDataJsonKey]!.ToObject<JObject>()!);

        var result = new AuthFlowSessionRecord(authorizationData, authCodeParameters!, id);

        return result;
    }
}
