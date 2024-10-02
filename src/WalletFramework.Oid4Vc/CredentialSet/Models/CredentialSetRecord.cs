using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;
using CredentialState = WalletFramework.Core.Credentials.CredentialState;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

public sealed class CredentialSetRecord : RecordBase
{
    public Option<Vct> SdJwtCredentialType { get; set; }

    public Option<DocType> MDocCredentialType { get; set; }

    public Dictionary<string, string> CredentialAttributes { get; set; } // Prioritizes Sd-Jwt

    public CredentialState State { get; set; } //ACTIVE, DELETED // MISSING: REVOKED, EXPIRED

    public DateTime ExpiresAt { get; set; }

    // public Option<StatusList> StatusList { get; }

    public Option<DateTime> RevokedAt { get; set; }

    public Option<DateTime> DeletedAt { get; set; }

    public string IssuerIdentifier { get; set; } //Problem (Contact is Lissi Domain Logic)

    /// <inheritdoc />
    public override string TypeName => "AF.CredentialSetRecord";
    
    public CredentialSetRecord()
    {
        Id = Guid.NewGuid().ToString();
    }
}

public static class CredentialSetRecordExtensions
{
    public static void AddSdJwtData(
        this CredentialSetRecord credentialSetRecord, 
        SdJwtRecord sdJwtRecord)
    {
        credentialSetRecord.SdJwtCredentialType = Vct.ValidVct(sdJwtRecord.Vct).ToOption();
        credentialSetRecord.CredentialAttributes = sdJwtRecord.Claims;
        credentialSetRecord.State = sdJwtRecord.CredentialState;
        credentialSetRecord.ExpiresAt = sdJwtRecord.ExpiresAt;
        credentialSetRecord.IssuerIdentifier = sdJwtRecord.IssuerId;
    }
    
    public static void AddMDocData(
        this CredentialSetRecord credentialSetRecord, 
        MdocRecord sdJwtRecord)
    {
        credentialSetRecord.MDocCredentialType = sdJwtRecord.DocType;
        // if (credentialSetRecord.CredentialAttributes.Count == 0)
        //     credentialSetRecord.CredentialAttributes = sdJwtRecord.Mdoc.IssuerSigned.IssuerNameSpaces;
        
        credentialSetRecord.State = sdJwtRecord.CredentialState;
    }
}
