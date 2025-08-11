// using Hyperledger.Aries.Storage;
// using Hyperledger.Aries.Storage.Models;
// using LanguageExt;
// using WalletFramework.Core.Credentials;
// using WalletFramework.Core.Credentials.Abstractions;
// using WalletFramework.Core.Cryptography.Models;
// using WalletFramework.Core.Functional;
// using WalletFramework.MdocLib;
// using WalletFramework.MdocVc.Display;
//
// namespace WalletFramework.MdocVc;
//
// public sealed class MdocRecord : RecordBase, ICredential
// {
//     public const int CurrentVersion = 2;
//     
//     public CredentialId CredentialId
//     {
//         get => CredentialId
//             .ValidCredentialId(Id)
//             .UnwrapOrThrow(new InvalidOperationException("The Id is corrupt"));
//         private set => Id = value;
//     }
//     
//     [RecordTag] 
//     public DocType DocType => Mdoc.DocType;
//     
//     public Mdoc Mdoc { get; }
//
//     public Option<List<MdocDisplay>> Displays { get; }
//     
//     public KeyId KeyId { get; }
//     
//     public CredentialState CredentialState { get; }
//     
//     /// <summary>
//     ///     Tracks whether it's a one-time use Mdoc.
//     /// </summary>
//     public bool OneTimeUse { get; set; }
//     
//     public Option<DateTime> ExpiresAt { get; }
//     
//     //TODO: Use CredentialSetId Type instead fo string
//     public string CredentialSetId
//     {
//         get => Get();
//         set => Set(value, false);
//     }
//
//     public override string TypeName => "WF.MdocRecord";
//
//     public MdocRecord(
//         Mdoc mdoc, 
//         Option<List<MdocDisplay>> displays, 
//         KeyId keyId, 
//         string credentialSetId, 
//         CredentialState credentialState, 
//         Option<DateTime> expiresAt,
//         bool isOneTimeUse = false)
//     {
//         CredentialId = CredentialId.CreateCredentialId();
//         Mdoc = mdoc;
//         Displays = displays;
//         KeyId = keyId;
//         CredentialSetId = credentialSetId;
//         CredentialState = credentialState;
//         ExpiresAt = expiresAt;
//         OneTimeUse = isOneTimeUse;
//         RecordVersion = CurrentVersion;
//     }
//
// #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//     public MdocRecord()
//     {
//     }
// #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//
//     public CredentialId GetId() => CredentialId;
//
//     public CredentialSetId GetCredentialSetId() => Core.Credentials.CredentialSetId
//         .ValidCredentialSetId(CredentialSetId)
//         .UnwrapOrThrow();
//
//     public static implicit operator Mdoc(MdocRecord record) => record.Mdoc;
// }
