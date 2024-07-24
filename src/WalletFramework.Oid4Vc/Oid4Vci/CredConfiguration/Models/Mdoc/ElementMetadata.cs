using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib.Elements;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.ElementMetadataFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record ElementMetadata
{
    public Option<bool> Mandatory { get; }

    public Option<List<ElementDisplay>> Display { get; }

    private ElementMetadata(Option<bool> mandatory, Option<List<ElementDisplay>> display)
    {
        Mandatory = mandatory;
        Display = display;
    }

    private static ElementMetadata CreateElementMetadata(JToken metadata)
    {
        var mandatory = metadata.GetByKey(MandatoryJsonKey).Match(
            jToken =>
            {
                var str = jToken.ToString();
                return bool.Parse(str);
            },
            _ => Option<bool>.None);

        var validDisplay =
            from token in metadata.GetByKey(DisplayJsonKey)
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
            token => ValidationFun.Valid(CreateElementMetadata(token))));
}

public static class ElementMetadataFun
{
    public const string MandatoryJsonKey = "mandatory";
    public const string DisplayJsonKey = "display";
    
    public static JObject EncodeToJson(this ElementMetadata metadata)
    {
        var result = new JObject();

        metadata.Mandatory.IfSome(
            mandatory => result.Add(MandatoryJsonKey, mandatory)
        );

        metadata.Display.IfSome(displays =>
        {
            var jArray = new JArray();
            foreach (var display in displays)
            {
                jArray.Add(display.EncodeToJson());
            }
            result.Add(DisplayJsonKey, jArray);
        });

        return result;
    }
}
