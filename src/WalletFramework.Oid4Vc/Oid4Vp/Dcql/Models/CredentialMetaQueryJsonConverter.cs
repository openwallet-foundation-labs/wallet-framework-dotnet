using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

public class CredentialMetaQueryJsonConverter : JsonConverter<CredentialMetaQuery>
{
    public override CredentialMetaQuery ReadJson(JsonReader reader, Type objectType, CredentialMetaQuery? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        
        return CredentialMetaQuery.FromJObject(jObject).Match(
            metadataQuery => metadataQuery,
            errors => throw new JsonSerializationException(
                $"Failed to deserialize CredentialMetaQuery: {string.Join(", ", errors.Select(e => e.Message))}")
        );
    }

    public override void WriteJson(JsonWriter writer, CredentialMetaQuery? value, JsonSerializer serializer)
    {
        var jObject = new JObject();
        
        if (value!.Vcts != null)
        {
            jObject[CredentialMetaFun.VctValuesJsonKey] = JToken.FromObject(value.Vcts, serializer);
        }
        
        if (value!.Doctype != null)
        {
            jObject[CredentialMetaFun.DoctypeValueJsonKey] = JToken.FromObject(value.Doctype, serializer);
        }
        
        jObject.WriteTo(writer);
    }
}
