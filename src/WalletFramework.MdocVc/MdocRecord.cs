using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Storage.Models;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using static WalletFramework.MdocVc.MdocRecordFun;

namespace WalletFramework.MdocVc;

[JsonConverter(typeof(MdocRecordJsonConverter))]
public sealed class MdocRecord : RecordBase, ICredential
{
    public CredentialId CredentialId
    {
        get => CredentialId
            .ValidCredentialId(Id)
            .UnwrapOrThrow(new InvalidOperationException("The Id is corrupt"));
        private set => Id = value;
    }

    [RecordTag] 
    public DocType DocType => Mdoc.DocType;
    
    public Mdoc Mdoc { get; }

    public Option<List<MdocDisplay>> Displays { get; }
    
    public KeyId KeyId { get; }
    
    public CredentialState CredentialState { get; }
    
    /// <summary>
    ///     Tracks whether it's a one-time use SD-JWT.
    /// </summary>
    public bool OneTimeUse { get; set; }
    
    public Option<DateTime> ExpiresAt { get; }
    
    [JsonIgnore]
    public CredentialSetId CredentialSetId
    {
        get => CredentialSetId.ValidCredentialSetId(Get())
            .UnwrapOrThrow(new InvalidOperationException("The CredentialSetId is corrupt"));
        set => Set(value, false);
    }

    public override string TypeName => "WF.MdocRecord";

    public MdocRecord(
        Mdoc mdoc, 
        Option<List<MdocDisplay>> displays, 
        KeyId keyId, 
        CredentialSetId credentialSetId, 
        CredentialState credentialState, 
        Option<DateTime> expiresAt,
        bool isOneTimeUse = false)
    {
        CredentialId = CredentialId.CreateCredentialId();
        Mdoc = mdoc;
        Displays = displays;
        KeyId = keyId;
        CredentialSetId = credentialSetId;
        CredentialState = credentialState;
        ExpiresAt = expiresAt;
        OneTimeUse = isOneTimeUse;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MdocRecord()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public CredentialId GetId() => CredentialId;
    
    public CredentialSetId GetCredentialSetId() => CredentialSetId;

    public static implicit operator Mdoc(MdocRecord record) => record.Mdoc;
}

public class MdocRecordJsonConverter : JsonConverter<MdocRecord>
{
    public override MdocRecord ReadJson(
        JsonReader reader,
        Type objectType,
        MdocRecord? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        return DecodeFromJson(json);
    }

    public override void WriteJson(JsonWriter writer, MdocRecord? value, JsonSerializer serializer)
    {
        var json = value!.EncodeToJson();
        json.WriteTo(writer);
    }
}

public static class MdocRecordFun
{
    private const string MdocDisplaysJsonKey = "displays";
    private const string MdocJsonKey = "mdoc";
    private const string KeyIdJsonKey = "keyId";
    private const string CredentialSetIdJsonKey = "credentialSetId";
    private const string CredentialStateJsonKey = "credentialState";
    private const string ExpiresAtJsonKey = "expiresAt";
    private const string OneTimeUseJsonKey = "oneTimeUse";

    public static MdocRecord DecodeFromJson(JObject json)
    {
        var id = json[nameof(RecordBase.Id)]!.ToString();
        
        var mdocStr = json[MdocJsonKey]!.ToString();
        var mdoc = Mdoc
            .ValidMdoc(mdocStr)
            .UnwrapOrThrow(new InvalidOperationException($"The MdocRecord with ID: {id} is corrupt"));
        
        var displays =
            from jToken in json.GetByKey(MdocDisplaysJsonKey).ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from mdocDisplays in MdocDisplayFun.DecodeFromJson(jArray)
            select mdocDisplays;

        var keyId = KeyId
            .ValidKeyId(json[KeyIdJsonKey]!.ToString())
            .UnwrapOrThrow();

        var credentialSetId = CredentialSetId.ValidCredentialSetId(json[CredentialSetIdJsonKey]!.ToString()).UnwrapOrThrow();
        
        var expiresAt = 
            from expires in json.GetByKey(ExpiresAtJsonKey).ToOption()
            select expires.ToObject<DateTime>();

        var credentialState = Enum.Parse<CredentialState>(json[CredentialStateJsonKey]!.ToString());
        
        var oneTimeUse = json.GetByKey(OneTimeUseJsonKey).ToOption().Match(
            Some: value => value.ToObject<bool>(),
            None: () => false
            );
        
        var result = new MdocRecord(mdoc, displays, keyId, credentialSetId, credentialState, expiresAt, oneTimeUse)
        {
            Id = id
        };

        return result;
    }

    public static JObject EncodeToJson(this MdocRecord record)
    {
        var result = new JObject
        {
            { nameof(RecordBase.Id), record.Id },
            { MdocJsonKey, record.Mdoc.Encode() },
            { KeyIdJsonKey, record.KeyId.ToString() },
            { CredentialSetIdJsonKey, record.CredentialSetId.ToString() },
            { CredentialStateJsonKey, record.CredentialState.ToString() },
            { OneTimeUseJsonKey, record.OneTimeUse }
        };
        
        record.ExpiresAt.IfSome(expires => result.Add(ExpiresAtJsonKey, expires));

        record.Displays.IfSome(displays =>
        {
            var displaysJson = new JArray();
            foreach (var display in displays)
            {
                displaysJson.Add(display.EncodeToJson());
            }

            result.Add(MdocDisplaysJsonKey, displaysJson);
        });
        
        return result;
    }

    public static MdocRecord ToRecord(this Mdoc mdoc, Option<List<MdocDisplay>> displays, KeyId keyId, CredentialSetId credentialSetId, bool isOneTimeUse) => 
        new(mdoc, displays, keyId, credentialSetId, CredentialState.Active, Option<DateTime>.None, isOneTimeUse);
}
