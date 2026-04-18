using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class ExpectedOriginsConverter : JsonConverter<Origin[]>
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override Origin[]? ReadJson(JsonReader reader, Type objectType,
        Origin[]? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var jArray = JArray.Load(reader);
            var origins = jArray
                .Select(token => token.ToString())
                .Where(origin => !string.IsNullOrEmpty(origin))
                .Select(origin => new Origin(origin))
                .ToArray();
            return origins;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, Origin[]? value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}
