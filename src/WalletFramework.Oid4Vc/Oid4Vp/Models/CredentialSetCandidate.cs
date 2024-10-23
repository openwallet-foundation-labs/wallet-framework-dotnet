using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class CredentialSetCandidate
{
    public CredentialSetId CredentialSetId { get; private set; }

    public List<ICredential> Credentials { get; private set; }

    public CredentialSetCandidate(CredentialSetId credentialSetId, IEnumerable<ICredential> credentials)
    {
        CredentialSetId = credentialSetId;
        Credentials = credentials.ToList();
    }
}
