using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc.Display;

namespace WalletFramework.MdocVc;

public static class MdocCredentialExtensions
{
    public static MdocCredential ToMdocCredential(
        this Mdoc mdoc,
        Option<List<MdocDisplay>> displays,
        KeyId keyId,
        CredentialSetId credentialSetId,
        CredentialState credentialState,
        bool oneTimeUse,
        Option<DateTime> expiresAt,
        CredentialId credentialId)
    {
        return new MdocCredential(mdoc, credentialId, credentialSetId, displays, keyId, credentialState, oneTimeUse, expiresAt);
    }

    public static string ToJsonString(this MdocCredential mdocCredential)
    {
        var json = JObject.FromObject(mdocCredential);
        return json.ToString();
    }
}
