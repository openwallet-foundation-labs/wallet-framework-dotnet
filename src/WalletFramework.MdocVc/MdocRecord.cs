using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Storage.Models;
using Hyperledger.Aries.Storage.Models.Interfaces;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;

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
    
    public Mdoc Mdoc { get; }
    
    [RecordTag] 
    public DocType DocType => Mdoc.DocType;
    
    public Option<List<MdocDisplay>> Displays { get; }

    public override string TypeName => "WF.MdocRecord";

    public MdocRecord(Mdoc mdoc, Option<List<MdocDisplay>> displays)
    {
        CredentialId = CredentialId.CreateCredentialId();
        Mdoc = mdoc;
        Displays = displays;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MdocRecord()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public static implicit operator Mdoc(MdocRecord record) => record.Mdoc;
}

public static class MdocRecordJsonKeys
{
    public const string MdocJsonKey = "mdoc";
    public const string MdocDisplaysKey = "displays";
}

public static class MdocRecordFun
{
    public static MdocRecord DecodeFromJson(JObject json)
    {
        var id = json[nameof(RecordBase.Id)]!.ToString();
        
        var mdocStr = json[MdocRecordJsonKeys.MdocJsonKey]!.ToString();
        var mdoc = Mdoc
            .ValidMdoc(mdocStr)
            .UnwrapOrThrow(new InvalidOperationException($"The MdocRecord with ID: {id} is corrupt"));

        var displays =
            from jToken in json.GetByKey(MdocRecordJsonKeys.MdocDisplaysKey).ToOption()
            from jArray in jToken.ToJArray().ToOption()
            from mdocDisplays in MdocDisplayFun.DecodeFromJson(jArray)
            select mdocDisplays;
        
        var result = new MdocRecord(mdoc, displays)
        {
            Id = id
        };

        return result;
    }

    public static MdocRecord ToRecord(this Mdoc mdoc, Option<List<MdocDisplay>> displays) => new(mdoc, displays);
}

public sealed class MdocRecordJsonConverter : JsonConverter<MdocRecord>
{
    public override void WriteJson(JsonWriter writer, MdocRecord? record, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(RecordBase.Id));
        writer.WriteValue(record!.Id);
        
        writer.WritePropertyName(MdocRecordJsonKeys.MdocJsonKey);
        writer.WriteValue(record.Mdoc.Encode());
        
        writer.WritePropertyName(MdocRecordJsonKeys.MdocDisplaysKey);
        record.Displays.Match(
            list =>
            {
                writer.WriteStartArray();
                foreach (var display in list)
                {
                    serializer.Serialize(writer, display);
                }
                writer.WriteEndArray();
            },
            () => {}
        );
        
        writer.WriteEndObject();
    }

    public override MdocRecord ReadJson(
        JsonReader reader,
        Type objectType,
        MdocRecord? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        return MdocRecordFun.DecodeFromJson(json);
    }
}
