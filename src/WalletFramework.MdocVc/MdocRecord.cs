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

    public override string TypeName => "WF.MdocRecord";

    public MdocRecord(Mdoc mdoc, Option<List<MdocDisplay>> displays, KeyId keyId)
    {
        CredentialId = CredentialId.CreateCredentialId();
        Mdoc = mdoc;
        Displays = displays;
        KeyId = keyId;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MdocRecord()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public CredentialId GetId() => CredentialId;

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
    public const string MdocJsonKey = "mdoc";
    public const string KeyIdJsonKey = "keyId";

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

        var result = new MdocRecord(mdoc, displays, keyId)
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
            { KeyIdJsonKey, record.KeyId.ToString() }
        };

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

    public static MdocRecord ToRecord(this Mdoc mdoc, Option<List<MdocDisplay>> displays, KeyId keyId) => 
        new(mdoc, displays, keyId);
}
