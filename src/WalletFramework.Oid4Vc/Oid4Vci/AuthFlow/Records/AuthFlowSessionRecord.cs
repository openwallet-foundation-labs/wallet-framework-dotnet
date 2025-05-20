using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Records;

/// <summary>
///   Represents the authorization session record. Used during the VCI Authorization Code Flow to hold session relevant information.
/// </summary>
[JsonConverter(typeof(AuthFlowSessionRecordConverter))]
public sealed class AuthFlowSessionRecord : RecordBase
{
    /// <summary>
    ///     The session specific state id.
    /// </summary>
    [JsonIgnore]
    public AuthFlowSessionState AuthFlowSessionState
    {
        get => AuthFlowSessionState
            .ValidAuthFlowSessionState(Id)
            .UnwrapOrThrow(new InvalidOperationException("AuthFlowSessionState is corrupt"));
        set
        {
            string str = value;
            Id = str;
        }
    }

    /// <summary>
    ///     The authorization data.
    /// </summary>
    public AuthorizationData AuthorizationData { get; set; }
        
    /// <summary>
    ///     The parameters for the 'authorization_code' grant type.
    /// </summary>
    public AuthorizationCodeParameters AuthorizationCodeParameters { get; }
    
    /// <summary>
    ///     Used to track the VCI Specification verison
    /// </summary>
    public int SpecVersion { get; }

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
    /// <param name="authFlowSessionState"></param>
    /// <param name="specVersion"></param>
    public AuthFlowSessionRecord(
        AuthorizationData authorizationData,
        AuthorizationCodeParameters authorizationCodeParameters,
        AuthFlowSessionState authFlowSessionState,
        int specVersion)
    {
        AuthFlowSessionState = authFlowSessionState;
        RecordVersion = 1;
        AuthorizationCodeParameters = authorizationCodeParameters;
        AuthorizationData = authorizationData;
        SpecVersion = specVersion;
    }
}

public class AuthFlowSessionRecordConverter : JsonConverter<AuthFlowSessionRecord>
{
    public override void WriteJson(JsonWriter writer, AuthFlowSessionRecord? value, JsonSerializer serializer)
    {
        var json = value!.EncodeToJson();
        json.WriteTo(writer);
    }

    public override AuthFlowSessionRecord ReadJson(
        JsonReader reader,
        Type objectType,
        AuthFlowSessionRecord? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        return AuthFlowSessionRecordFun.DecodeFromJson(json);
    }
}

public static class AuthFlowSessionRecordFun
{
    private const string AuthorizationDataJsonKey = "authorization_data";
    private const string AuthorizationCodeParametersJsonKey = "authorization_code_parameters";
    private const string SpecVersionJsonKey = "spec_version";

    public static JObject EncodeToJson(this AuthFlowSessionRecord record)
    {
        var authorizationData = record.AuthorizationData.EncodeToJson();
        var authorizationCodeParameters = JObject.FromObject(record.AuthorizationCodeParameters);

        return new JObject
        {
            { nameof(RecordBase.Id), record.Id },
            { AuthorizationDataJsonKey, authorizationData },
            { AuthorizationCodeParametersJsonKey, authorizationCodeParameters },
            { SpecVersionJsonKey, record.SpecVersion }
        };
    }

    public static AuthFlowSessionRecord DecodeFromJson(JObject json)
    {
        var idJson = json[nameof(RecordBase.Id)]!.ToObject<JValue>()!;
        var id = AuthFlowSessionStateFun.DecodeFromJson(idJson);

        var authCodeParameters = JsonConvert.DeserializeObject<AuthorizationCodeParameters>(
            json[AuthorizationCodeParametersJsonKey]!.ToString()
        );

        var authorizationData = AuthorizationDataFun
            .DecodeFromJson(json[AuthorizationDataJsonKey]!.ToObject<JObject>()!);
        
        var specVersion = json[SpecVersionJsonKey]!.ToObject<int>();
        
        var result = new AuthFlowSessionRecord(authorizationData, authCodeParameters!, id, specVersion);

        return result;
    }
}
