using Hyperledger.Aries.Storage;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Used to persist OpenId4Vp presentations.
/// </summary>
[JsonConverter(typeof(OidPresentationRecordConverter))]
public sealed class OidPresentationRecord : RecordBase
{
    /// <summary>
    ///     Gets or sets the credentials the Holder presented to the Verifier.
    /// </summary>
    public List<PresentedCredentialSet> PresentedCredentialSets { get; set; }

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
    public Option<ClientMetadata> ClientMetadata { get; set; }
    
    /// <summary>
    ///     Gets or sets the name of the presentation.
    /// </summary>
    [JsonIgnore]
    public Option<string> Name
    {
        get => Get();
        set => value.Match(
            name =>
                {
                    Set(name, false);
                },
            () => { });
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
    /// <param name="presentedCredentialSets">The credential sets the Holder presented to the Verifier.</param>
    /// <param name="clientId">The client id for the Verifier.</param>
    /// <param name="id">The id of the record.</param>
    /// <param name="clientMetadata">The metadata of the Verifier.</param>
    /// <param name="name">The name of the presentation.</param>
    public OidPresentationRecord(
        List<PresentedCredentialSet> presentedCredentialSets,
        string clientId,
        string id,
        Option<ClientMetadata> clientMetadata,
        Option<string> name)
    {
        ClientId = clientId;
        ClientMetadata = clientMetadata;
        Id = id;
        Name = name;
        PresentedCredentialSets = presentedCredentialSets;
    }
}

public class OidPresentationRecordConverter : JsonConverter<OidPresentationRecord>
{
    public override void WriteJson(JsonWriter writer, OidPresentationRecord? value, JsonSerializer serializer)
    {
        var json = value!.EncodeToJson();
        json.WriteTo(writer);
    }
    
    public override OidPresentationRecord ReadJson(
        JsonReader reader,
        Type objectType,
        OidPresentationRecord? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        return OidPresentationRecordExtensions.DecodeFromJson(json);
    }
}

public static class OidPresentationRecordExtensions
{
    private const string PresentedCredentialSetsJsonKey = "presented_credential_sets";
    private const string ClientMetadataJsonKey = "client_metadata";
    private const string ClientIdJsonKey = "client_id";
    private const string NameJsonKey = "name";
    
    public static JObject EncodeToJson(this OidPresentationRecord credentialSetRecord)
    {
        var result = new JObject();
        
        result.Add(nameof(RecordBase.Id), credentialSetRecord.Id);
        
        result.Add(ClientIdJsonKey, credentialSetRecord.ClientId);
        
        var presentedCredentialSetArray = new JArray();
        foreach (var presentedCredentialSet in credentialSetRecord.PresentedCredentialSets)
        {
            presentedCredentialSetArray.Add(presentedCredentialSet.EncodeToJson());
        }
        result.Add(PresentedCredentialSetsJsonKey, presentedCredentialSetArray);
        
        credentialSetRecord.ClientMetadata.IfSome(clientMetadata => result.Add(ClientMetadataJsonKey, JsonConvert.SerializeObject(clientMetadata)));
        
        credentialSetRecord.Name.IfSome(name => result.Add(NameJsonKey, name));
    
        return result;
    }

    public static OidPresentationRecord DecodeFromJson(JObject json)
    {
        var id = json[nameof(RecordBase.Id)]!.ToString();

        var clientId = json[ClientIdJsonKey]!.ToString();
        
        var presentedCredentialSets =
            from jToken in json.GetByKey(PresentedCredentialSetsJsonKey).ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from presentedCredentialSet in PresentedCredentialSetExtensions.DecodeFromJson(jArray)
            select presentedCredentialSet;

        var clientMetadata =
            from jToken in json.GetByKey(ClientMetadataJsonKey).ToOption()
            select JsonConvert.DeserializeObject<ClientMetadata>(jToken.ToString());

        var name =
            from jToken in json.GetByKey(NameJsonKey).ToOption()
            select jToken.ToString();

        return new OidPresentationRecord(
            presentedCredentialSets.ToList(),
            clientId, 
            id, 
            clientMetadata, 
            name);
    }
}
