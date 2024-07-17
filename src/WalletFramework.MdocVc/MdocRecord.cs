using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Storage.Models;
using Hyperledger.Aries.Storage.Models.Interfaces;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using static WalletFramework.MdocVc.MdocRecordFun;

namespace WalletFramework.MdocVc;

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

public static class MdocRecordFun
{
    public const string MdocJsonKey = "mdoc";
    private const string MdocDisplaysJsonKey = "displays";
    
    public static JObject EncodeToJson(this MdocRecord record)
    {
        var result = new JObject
        {
            {nameof(RecordBase.Id), record.Id},
            {MdocJsonKey, record.Mdoc.Encode()}
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
        
        var result = new MdocRecord(mdoc, displays)
        {
            Id = id
        };

        return result;
    }

    public static MdocRecord ToRecord(this Mdoc mdoc, Option<List<MdocDisplay>> displays) => new(mdoc, displays);
}
