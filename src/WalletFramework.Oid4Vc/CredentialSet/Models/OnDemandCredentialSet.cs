using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

public record OnDemandCredentialSet(CredentialDataSet CredentialDataSet, List<ICredential> Credentials);
