// using Hyperledger.Aries.Storage;
// using Newtonsoft.Json.Linq;
// using WalletFramework.Core.Credentials;
// using WalletFramework.Core.Cryptography.Models;
// using WalletFramework.Core.Functional;
// using WalletFramework.Core.Json;
// using WalletFramework.MdocLib;
// using WalletFramework.MdocVc.Display.Serialization;
// using static WalletFramework.MdocVc.Serialization.MdocRecordSerializationConstants;
//
// namespace WalletFramework.MdocVc.Serialization;
//
// public static class MdocRecordSerializer
// {
//     public static string Serialize(MdocRecord record)
//     {
//         var json = EncodeToJson(record);
//         return json.ToString();
//     }
//
//     public static Validation<MdocRecord> Deserialize(string jsonString)
//     {
//         var json = JObject.Parse(jsonString);
//         return DecodeFromJson(json);
//     }
//
//     private static MdocRecord DecodeFromJson(JObject json)
//     {
//         var id = json[nameof(RecordBase.Id)]!.ToString();
//         var recordVersion = json.GetByKey(RecordVersionJsonKey).ToOption().Match(
//             value => value.ToObject<int>(),
//             () => 1);
//         
//         var mdocStr = json[MdocJsonKey]!.ToString();
//         var mdoc = Mdoc
//             .ValidMdoc(mdocStr)
//             .UnwrapOrThrow();
//         
//         var displays =
//             from jToken in json.GetByKey(MdocDisplaysJsonKey).ToOption()
//             from jArray in jToken.ToJArray().ToOption()
//             from mdocDisplays in jArray.TraverseAll(
//                 token => MdocDisplaySerializer.Deserialize(token.ToString()).ToOption())
//             select mdocDisplays.ToList();
//
//         var keyId = KeyId
//             .ValidKeyId(json[KeyIdJsonKey]!.ToString())
//             .UnwrapOrThrow();
//
//         var credentialSetId = recordVersion >= 2 ? json[CredentialSetIdJsonKey]!.ToObject<string>()! : string.Empty;
//         
//         var expiresAt = 
//             from expires in json.GetByKey(ExpiresAtJsonKey).ToOption()
//             select expires.ToObject<DateTime>();
//
//         var credentialState = recordVersion >= 2
//             ? Enum.Parse<CredentialState>(json[CredentialStateJsonKey]!.ToString())
//             : CredentialState.Active;
//         
//         var oneTimeUse = json.GetByKey(OneTimeUseJsonKey).ToOption().Match(
//             Some: value => value.ToObject<bool>(),
//             None: () => false);
//         
//         var result = new MdocRecord(mdoc, displays, keyId, credentialSetId, credentialState, expiresAt, oneTimeUse)
//         {
//             Id = id,
//             RecordVersion = recordVersion
//         };
//
//         return result;
//     }
//
//     private static JObject EncodeToJson(this MdocRecord record)
//     {
//         var result = new JObject
//         {
//             { nameof(RecordBase.Id), record.Id },
//             { MdocJsonKey, record.Mdoc.Encode() },
//             { KeyIdJsonKey, record.KeyId.ToString() },
//             { CredentialSetIdJsonKey, record.CredentialSetId },
//             { CredentialStateJsonKey, record.CredentialState.ToString() },
//             { OneTimeUseJsonKey, record.OneTimeUse }
//         };
//         
//         record.ExpiresAt.IfSome(expires => result.Add(ExpiresAtJsonKey, expires));
//
//         record.Displays.IfSome(displays =>
//         {
//             var displaysJson = new JArray();
//             foreach (var display in displays)
//             {
//                 displaysJson.Add(MdocDisplaySerializer.Serialize(display));
//             }
//
//             result.Add(MdocDisplaysJsonKey, displaysJson);
//         });
//         
//         return result;
//     }
// }
