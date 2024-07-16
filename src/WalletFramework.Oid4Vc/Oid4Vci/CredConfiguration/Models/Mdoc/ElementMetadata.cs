using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.MdocLib;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record ElementMetadata
{
    [JsonProperty("mandatory")]
    [JsonConverter(typeof(OptionJsonConverter<bool>))]
    public Option<bool> Mandatory { get; }

    [JsonProperty("display")]
    [JsonConverter(typeof(OptionJsonConverter<List<ElementDisplay>>))]
    public Option<List<ElementDisplay>> Display { get; }

    private ElementMetadata(Option<bool> mandatory, Option<List<ElementDisplay>> display)
    {
        Mandatory = mandatory;
        Display = display;
    }

    public static ElementMetadata CreateElementMetadata(JToken metadata)
    {
        var mandatory = metadata.GetByKey("mandatory").Match(
            jToken =>
            {
                var str = jToken.ToString();
                return bool.Parse(str);
            },
            _ => Option<bool>.None);

        var validDisplay =
            from token in metadata.GetByKey("display")
            from array in token.ToJArray()
            select array;

        var display =
            from array in validDisplay.ToOption()
            from displays in array.TraverseAny(token =>
            {
                var optJObject = token.ToJObject().ToOption();
                return 
                    from jObject in optJObject
                    from elementDisplay in ElementDisplay.OptionalElementDisplay(jObject)
                    select elementDisplay;
            })
            select displays.ToList();

        return new ElementMetadata(mandatory, display);
    }
    
    public static Validation<Dictionary<ElementIdentifier, ElementMetadata>> ValidElementMetadatas(
        JToken metadatas) => metadatas
        .ToJObject()
        .OnSuccess(o => o.ToValidDictionaryAll(
            ElementIdentifier.ValidElementIdentifier,
            token => ValidationFun.Valid(ElementMetadata.CreateElementMetadata(token))));
}
