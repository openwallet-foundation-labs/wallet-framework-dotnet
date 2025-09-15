using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

public record OnDemandCredentialSet(CredentialSetRecord CredentialSetRecord, List<ICredential> CredentialRecords);
