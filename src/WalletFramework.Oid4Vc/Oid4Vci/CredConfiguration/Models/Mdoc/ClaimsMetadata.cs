using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocVc;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.ElementMetadata;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public readonly struct ClaimsMetadata
{
    public Dictionary<NameSpace, Dictionary<ElementIdentifier, ElementMetadata>> Value { get; }

    private ClaimsMetadata(Dictionary<NameSpace, Dictionary<ElementIdentifier, ElementMetadata>> value) =>
        Value = value;

    public static Validation<ClaimsMetadata> ValidClaimsMetadata(JObject claims) => claims
        .ToValidDictionaryAll(NameSpace.ValidNameSpace, ValidElementMetadatas)
        .OnSuccess(dictionary => new ClaimsMetadata(dictionary));
}

public static class ClaimsMetadataFun
{
    public static JObject EncodeToJson(this ClaimsMetadata metadata)
    {
        var result = new JObject();

        foreach (var (key, elementMetadatas) in metadata.Value)
        {
            var elementsJson = new JObject();
            foreach (var (elementIdentifier, elementMetadata) in elementMetadatas)
            {
                elementsJson.Add(elementIdentifier, elementMetadata.EncodeToJson());
            }
            
            result.Add(key.ToString(), elementsJson);
        }

        return result;
    }
    
    public static Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>> ToClaimsDisplays(
        this ClaimsMetadata claimsMetadata)
    {
        var result = new Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>();

        foreach (var nameSpacePair in claimsMetadata.Value)
        {
            var elementDisplays = new Dictionary<ElementIdentifier, List<ClaimDisplay>>();
            foreach (var elementPair in nameSpacePair.Value)
            {
                elementPair.Value.Display.Match(
                    list =>
                    {
                        var displays = list.Select(elementDisplay =>
                        {
                            var name =
                                from elementName in elementDisplay.Name
                                from claimName in ClaimName.OptionClaimName(elementName)
                                select claimName;

                            return new ClaimDisplay(name, elementDisplay.Locale);
                        }).ToList();
                        elementDisplays.Add(elementPair.Key, displays);
                    },
                    () => {}
                );
            }

            if (elementDisplays.Any()) 
                result.Add(nameSpacePair.Key, elementDisplays);
        }

        if (result.Any())
        {
            return result;
        }
        else
        {
            return Option<Dictionary<NameSpace, Dictionary<ElementIdentifier, List<ClaimDisplay>>>>.None;
        }
    }
}
