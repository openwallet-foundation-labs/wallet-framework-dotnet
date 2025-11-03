using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib;

namespace WalletFramework.MdocVc;

public static class MdocCredentialExtensions
{
    public static MdocCredential ToMdocCredential(
        this Mdoc mdoc,
        KeyId keyId,
        CredentialSetId credentialSetId,
        CredentialState credentialState,
        bool oneTimeUse,
        Option<DateTime> expiresAt,
        CredentialId credentialId)
    {
        return new MdocCredential(mdoc, credentialId, credentialSetId, keyId, credentialState, oneTimeUse, expiresAt);
    }

    public static string ToJsonString(this MdocCredential mdocCredential)
    {
        var json = JObject.FromObject(mdocCredential);
        return json.ToString();
    }
}
