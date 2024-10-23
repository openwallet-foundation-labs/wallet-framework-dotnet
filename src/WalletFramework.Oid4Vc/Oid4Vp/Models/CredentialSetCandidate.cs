using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class CredentialSetCandidate(CredentialSetId credentialSetId, IEnumerable<ICredential> credentials)
{
    public CredentialSetId CredentialSetId { get; private set; } = credentialSetId;

    public List<ICredential> Credentials { get; private set; } = credentials.ToList();
}
