using Hyperledger.Aries.Storage;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;
using CredentialState = WalletFramework.Core.Credentials.CredentialState;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

[JsonConverter(typeof(CredentialSetRecordConverter))]
public sealed class CredentialSetRecord : RecordBase
{
    public Option<Vct> SdJwtCredentialType { get; set; }

    public Option<DocType> MDocCredentialType { get; set; }

    public Dictionary<string, string> CredentialAttributes { get; set; } = null!;

    public CredentialState State { get; set; }

    public Option<DateTime> ExpiresAt { get; set; }

    //TODO: Add Status List
    // public Option<StatusList> StatusList { get; }

    public Option<DateTime> RevokedAt { get; set; }

    public Option<DateTime> DeletedAt { get; set; }

    [JsonIgnore]
    public string IssuerId
    {
        get => Get();
        set => Set(value, false);
    }

    /// <inheritdoc />
    public override string TypeName => "AF.CredentialSetRecord";

    public CredentialSetRecord(
        Option<Vct> sdJwtCredentialType, 
        Option<DocType> mDocCredentialType, 
        Dictionary<string, string> credentialAttributes, 
        CredentialState credentialState,
        Option<DateTime> expiresAt, 
        Option<DateTime> revokedAt, 
        Option<DateTime> deletedAt, 
        string issuerId)
    {
        SdJwtCredentialType = sdJwtCredentialType;
        MDocCredentialType = mDocCredentialType;
        CredentialAttributes = credentialAttributes;
        State = credentialState;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
        DeletedAt = deletedAt;
        IssuerId = issuerId;
    }
    
    public CredentialSetRecord()
    {
        Id = CredentialSetId.CreateCredentialSetId();
    }
}

public class CredentialSetRecordConverter : JsonConverter<CredentialSetRecord>
{
    public override void WriteJson(JsonWriter writer, CredentialSetRecord? value, JsonSerializer serializer)
    {
        var json = value!.EncodeToJson();
        json.WriteTo(writer);
    }
    
    public override CredentialSetRecord ReadJson(
        JsonReader reader,
        Type objectType,
        CredentialSetRecord? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        return CredentialSetRecordExtensions.DecodeFromJson(json);
    }
}

public static class CredentialSetRecordExtensions
{
    private const string CredentialSetIdTypeJsonKey = "credential_set_id";
    private const string SdJwtCredentialTypeJsonKey = "sd_jwt_credential_type";
    private const string MDocCredentialTypeJsonKey = "mdoc_credential_type";
    private const string CredentialAttributesJsonKey = "credential_attributes";
    private const string StateJsonKey = "state";
    private const string ExpiresAtJsonKey = "expires_at";
    private const string RevokedAtJsonKey = "revoked_at";
    private const string DeletedAtJsonKey = "deleted_at";
    private const string IssuerIdJsonKey = "issuer_id";
    
    public static void AddSdJwtData(
        this CredentialSetRecord credentialSetRecord, 
        SdJwtRecord sdJwtRecord)
    {
        credentialSetRecord.SdJwtCredentialType = Vct.ValidVct(sdJwtRecord.Vct).ToOption();
        credentialSetRecord.CredentialAttributes = sdJwtRecord.Claims;
        credentialSetRecord.State = sdJwtRecord.CredentialState;
        credentialSetRecord.ExpiresAt = sdJwtRecord.ExpiresAt.ToOption();
        credentialSetRecord.IssuerId = sdJwtRecord.IssuerId;
    }
    
    public static void AddMDocData(
        this CredentialSetRecord credentialSetRecord, 
        MdocRecord mdocRecord)
    {
        credentialSetRecord.MDocCredentialType = mdocRecord.DocType;
        
        if (credentialSetRecord.CredentialAttributes.Count == 0) 
            credentialSetRecord.CredentialAttributes = mdocRecord.Mdoc.IssuerSigned.IssuerNameSpaces.Value.First().Value.ToDictionary(
                issuerSignedItem => issuerSignedItem.ElementId.ToString(), 
                issuerSignedItem => issuerSignedItem.Element.ToString());
        
        credentialSetRecord.State = mdocRecord.CredentialState;
        credentialSetRecord.ExpiresAt = mdocRecord.ExpiresAt.ToOption();
        //TODO: Add issuerId
    }
    
    public static CredentialSetId GetCredentialSetId(this CredentialSetRecord credentialSetRecord) =>
        CredentialSetId.ValidCredentialSetId(credentialSetRecord.Id)
            .UnwrapOrThrow(new InvalidOperationException("The Id is corrupt"));
    
    public static OneOf<Vct, DocType> GetCredentialType(this CredentialSetRecord credentialSetRecord)
    {
        return credentialSetRecord.SdJwtCredentialType.Match(
            vct => vct,
            () => credentialSetRecord.MDocCredentialType.Match<OneOf<Vct, DocType>>(
                docType => docType,
                () => throw new InvalidOperationException("No credential type found")));
    }
    
    public static JObject EncodeToJson(this CredentialSetRecord credentialSetRecord)
    {
        var result = new JObject();
        
        result.Add(nameof(RecordBase.Id), credentialSetRecord.Id);
        
        credentialSetRecord.SdJwtCredentialType.IfSome(vct => result.Add(SdJwtCredentialTypeJsonKey, vct.ToString()));
        credentialSetRecord.MDocCredentialType.IfSome(docType => result.Add(MDocCredentialTypeJsonKey, docType.ToString()));
        
        var jObjectDictionary = new JObject();
        foreach (var kvp in credentialSetRecord.CredentialAttributes)
        {
            jObjectDictionary.Add(kvp.Key, kvp.Value);
        }
        result.Add(CredentialAttributesJsonKey, jObjectDictionary);
        
        result.Add(StateJsonKey, credentialSetRecord.State.ToString());
        
        credentialSetRecord.ExpiresAt.IfSome(expiresAt => result.Add(ExpiresAtJsonKey, expiresAt));
        credentialSetRecord.RevokedAt.IfSome(revokedAt => result.Add(RevokedAtJsonKey, revokedAt));
        credentialSetRecord.DeletedAt.IfSome(deletedAt => result.Add(DeletedAtJsonKey, deletedAt));
        result.Add(IssuerIdJsonKey, credentialSetRecord.IssuerId);

        return result;
    }
    
    public static CredentialSetRecord DecodeFromJson(JObject json)
    {
        var id = json[nameof(RecordBase.Id)]!.ToString();

        var sdJwtCredentialType = 
            from jToken in json.GetByKey(SdJwtCredentialTypeJsonKey).ToOption()
            from vct in Vct.ValidVct(jToken).ToOption()
            select vct;
        
        var mDocCredentialType = 
            from jToken in json.GetByKey(MDocCredentialTypeJsonKey).ToOption()
            from docType in DocType.ValidDoctype(jToken).ToOption()
            select docType;

        var credentialAttributesType = json[CredentialAttributesJsonKey]!.ToObject<Dictionary<string, string>>()!;
        
        var stateType = Enum.Parse<CredentialState>(json[StateJsonKey]!.ToString());

        var expiresAtType = 
            from jToken in json.GetByKey(ExpiresAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var revokedAtType =
            from jToken in json.GetByKey(RevokedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var deletedAtType =
            from jToken in json.GetByKey(DeletedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var issuerIdentifierType = json[IssuerIdJsonKey]!.ToString();

        return new CredentialSetRecord(
            sdJwtCredentialType,
            mDocCredentialType,
            credentialAttributesType,
            stateType,
            expiresAtType,
            revokedAtType,
            deletedAtType,
            issuerIdentifierType)
        {
            Id = id
        };
    }
}
