using Hyperledger.Aries.Storage;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.String;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Models.StatusList;
using CredentialState = WalletFramework.Core.Credentials.CredentialState;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

[JsonConverter(typeof(CredentialSetRecordConverter))]
public sealed class CredentialSetRecord : RecordBase
{
    public Option<Vct> SdJwtCredentialType { get; set; }

    public Option<DocType> MDocCredentialType { get; set; }

    public Dictionary<string, string> CredentialAttributes { get; set; } = new();
    
    public CredentialState State { get; set; }

    public Option<DateTime> NotBefore { get; set; }
    
    public Option<DateTime> IssuedAt { get; set; }
    
    public Option<DateTime> ExpiresAt { get; set; }

    public Option<Status> StatusList { get; set; }

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
        Option<Status> statusList,
        Option<DateTime> expiresAt,
        Option<DateTime> issuedAt,
        Option<DateTime> notBefore,
        Option<DateTime> revokedAt, 
        Option<DateTime> deletedAt,
        Option<DateTime> createdAt, 
        Option<DateTime> updatedAt, 
        string issuerId)
    {
        SdJwtCredentialType = sdJwtCredentialType;
        MDocCredentialType = mDocCredentialType;
        CredentialAttributes = credentialAttributes;
        State = credentialState;
        StatusList = statusList;
        ExpiresAt = expiresAt;
        IssuedAt = issuedAt;
        NotBefore = notBefore;
        RevokedAt = revokedAt;
        DeletedAt = deletedAt;
        UpdatedAtUtc = updatedAt.ToNullable();
        CreatedAtUtc = createdAt.ToNullable();
        IssuerId = issuerId;
    }
    
    public CredentialSetRecord()
    {
        Id = CredentialSetId.CreateCredentialSetId();
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
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
    private const string IssuedAtJsonKey = "issued_at";
    private const string NotBeforeJsonKey = "not_before";
    private const string RevokedAtJsonKey = "revoked_at";
    private const string DeletedAtJsonKey = "deleted_at";
    private const string IssuerIdJsonKey = "issuer_id";
    private const string CreatedAtJsonKey = "created_at";
    private const string UpdatedAtJsonKey = "updated_at";
    private const string StatusListJsonKey = "status_list";
    
    public static void AddSdJwtData(
        this CredentialSetRecord credentialSetRecord, 
        SdJwtRecord sdJwtRecord)
    {
        credentialSetRecord.SdJwtCredentialType = Vct.ValidVct(sdJwtRecord.Vct).ToOption();
        credentialSetRecord.CredentialAttributes = sdJwtRecord.Claims;
        credentialSetRecord.State = sdJwtRecord.CredentialState;
        credentialSetRecord.ExpiresAt = sdJwtRecord.ExpiresAt.ToOption();
        credentialSetRecord.IssuedAt = sdJwtRecord.IssuedAt.ToOption();
        credentialSetRecord.NotBefore = sdJwtRecord.NotBefore.ToOption();
        credentialSetRecord.IssuerId = sdJwtRecord.IssuerId;
        credentialSetRecord.StatusList = sdJwtRecord.Status;
    }
    
    public static void AddMDocData(
        this CredentialSetRecord credentialSetRecord, 
        MdocRecord mdocRecord,
        CredentialIssuerId credentialIssuerId)
    {
        credentialSetRecord.MDocCredentialType = mdocRecord.DocType;
        credentialSetRecord.State = mdocRecord.CredentialState;
        
        if (credentialSetRecord.CredentialAttributes.Count == 0) 
            credentialSetRecord.CredentialAttributes = mdocRecord.Mdoc.IssuerSigned.IssuerNameSpaces.Value.First().Value.ToDictionary(
                issuerSignedItem => issuerSignedItem.ElementId.ToString(), 
                issuerSignedItem => issuerSignedItem.Element.ToString());
        
        if (credentialSetRecord.ExpiresAt.IsNone)
            credentialSetRecord.ExpiresAt = mdocRecord.ExpiresAt.ToOption();

        if (credentialSetRecord.IssuerId.IsNullOrEmpty())
            credentialSetRecord.IssuerId = credentialIssuerId.ToString();
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
    
    public static bool IsActive(this CredentialSetRecord credentialSetRecord) =>
        credentialSetRecord.State == CredentialState.Active;
    
    public static bool IsRevoked(this CredentialSetRecord credentialSetRecord) =>
        credentialSetRecord.State == CredentialState.Revoked;
    
    public static bool IsDeleted(this CredentialSetRecord credentialSetRecord) => 
        credentialSetRecord.State == CredentialState.Deleted;
    
    public static bool IsExpired(this CredentialSetRecord credentialSetRecord) =>
        credentialSetRecord.State == CredentialState.Expired;
    
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
        credentialSetRecord.IssuedAt.IfSome(issuedAt => result.Add(IssuedAtJsonKey, issuedAt));
        credentialSetRecord.NotBefore.IfSome(notBefore => result.Add(NotBeforeJsonKey, notBefore));
        credentialSetRecord.RevokedAt.IfSome(revokedAt => result.Add(RevokedAtJsonKey, revokedAt));
        credentialSetRecord.DeletedAt.IfSome(deletedAt => result.Add(DeletedAtJsonKey, deletedAt));
        credentialSetRecord.CreatedAtUtc.IfSome(createdAtUtc => result.Add(CreatedAtJsonKey, createdAtUtc));
        credentialSetRecord.UpdatedAtUtc.IfSome(updatedAtUtc => result.Add(UpdatedAtJsonKey, updatedAtUtc));
        credentialSetRecord.StatusList.IfSome(statusList => result.Add(StatusListJsonKey, JObject.FromObject(statusList)));
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

        var statusListType = 
            from jToken in json.GetByKey(StatusListJsonKey).ToOption()
            select jToken.ToObject<Status>();
        
        var expiresAtType = 
            from jToken in json.GetByKey(ExpiresAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var issuedAtType = 
            from jToken in json.GetByKey(IssuedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var notBeforeType = 
            from jToken in json.GetByKey(NotBeforeJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var revokedAtType =
            from jToken in json.GetByKey(RevokedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var deletedAtType =
            from jToken in json.GetByKey(DeletedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var createdAtType =
            from jToken in json.GetByKey(CreatedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var updatedAtType =
            from jToken in json.GetByKey(UpdatedAtJsonKey).ToOption()
            select jToken.ToObject<DateTime>();
        
        var issuerIdentifierType = json[IssuerIdJsonKey]!.ToString();

        return new CredentialSetRecord(
            sdJwtCredentialType,
            mDocCredentialType,
            credentialAttributesType,
            stateType,
            statusListType,
            expiresAtType,
            issuedAtType,
            notBeforeType,
            revokedAtType,
            deletedAtType,
            createdAtType,
            updatedAtType,
            issuerIdentifierType)
        {
            Id = id
        };
    }
}
