using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc.Display.Serialization;
using static WalletFramework.MdocVc.Serialization.MdocCredentialSerializationConstants;

namespace WalletFramework.MdocVc.Serialization;

public static class MdocCredentialSerializer
{
    public static string Serialize(MdocCredential credential)
    {
        var json = EncodeToJObject(credential);
        return json.ToString();
    }

    public static Validation<MdocCredential> Deserialize(string jsonString)
    {
        var json = JObject.Parse(jsonString);
        return DecodeFromJObject(json);
    }

    private static JObject EncodeToJObject(this MdocCredential credential)
    {
        var result = new JObject
        {
            { MdocJsonKey, credential.Mdoc.Encode() },
            { CredentialIdJsonKey, credential.CredentialId.AsString() },
            { CredentialSetIdJsonKey, credential.CredentialSetId.AsString() },
            { KeyIdJsonKey, credential.KeyId.AsString() },
            { CredentialStateJsonKey, credential.CredentialState.ToString() },
            { OneTimeUseJsonKey, credential.OneTimeUse }
        };

        credential.ExpiresAt.IfSome(expires => result.Add(ExpiresAtJsonKey, expires));

        credential.Displays.IfSome(displays =>
        {
            var array = new JArray(displays.Select(MdocDisplaySerializer.Serialize));
            result.Add(DisplaysJsonKey, array);
        });

        return result;
    }

    private static MdocCredential DecodeFromJObject(JObject json)
    {
        var mdocStr = json[MdocJsonKey]!.ToString();
        var mdoc = Mdoc.ValidMdoc(mdocStr).UnwrapOrThrow();

        var credentialId = CredentialId
            .ValidCredentialId(json[CredentialIdJsonKey]!.ToString())
            .UnwrapOrThrow();

        var credentialSetId = CredentialSetId
            .ValidCredentialSetId(json[CredentialSetIdJsonKey]!.ToString())
            .UnwrapOrThrow();

        var keyId = KeyId
            .ValidKeyId(json[KeyIdJsonKey]!.ToString())
            .UnwrapOrThrow();

        var credentialState = Enum.Parse<CredentialState>(json[CredentialStateJsonKey]!.ToString());

        var oneTimeUse = json.GetByKey(OneTimeUseJsonKey).ToOption().Match(
            Some: value => value.ToObject<bool>(),
            None: () => false);

        var expiresAt =
            from ex in json.GetByKey(ExpiresAtJsonKey).ToOption()
            select ex.ToObject<DateTime>();

        var displays =
            from token in json.GetByKey(DisplaysJsonKey).ToOption()
            from array in token.ToJArray().ToOption()
            from list in array.TraverseAll(t => MdocDisplaySerializer.Deserialize(t.ToString()).ToOption())
            select list.ToList();

        return new MdocCredential(
            mdoc,
            credentialId,
            credentialSetId,
            displays,
            keyId,
            credentialState,
            oneTimeUse,
            expiresAt);
    }
}
