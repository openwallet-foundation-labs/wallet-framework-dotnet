using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class CredentialSetCandidate
{
    public string CredentialSetId { get; private set; }

    public List<ICredential> Credentials { get; private set; }

    public CredentialSetCandidate(string credentialSetId, IEnumerable<ICredential> credentials)
    {
        CredentialSetId = credentialSetId;
        Credentials = credentials.ToList();
    }
}
