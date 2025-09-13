using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp;

public record CompletedPresentation(
    string PresentationId,
    string ClientId,
    List<PresentedCredentialSet> PresentedCredentialSets,
    Option<ClientMetadata> ClientMetadata,
    Option<string> Name,
    DateTimeOffset LastTimeUsed);

public static class CompletedPresentationExtensions
{
    private const string PresentationIdJsonKey = "PresentationId";
    private const string ClientIdJsonKey = "ClientId";
    private const string PresentedCredentialSetsJsonKey = "PresentedCredentialSets";
    private const string ClientMetadataJsonKey = "ClientMetadata";
    private const string NameJsonKey = "Name";
    private const string LastTimeUsedJsonKey = "LastTimeUsed";

    public static JObject EncodeToJson(this CompletedPresentation presentation)
    {
        var result = new JObject
        {
            { PresentationIdJsonKey, presentation.PresentationId },
            { ClientIdJsonKey, presentation.ClientId }
        };

        var presentedSetsArray = new JArray(presentation.PresentedCredentialSets.Select(set => set.EncodeToJson()));
        result.Add(PresentedCredentialSetsJsonKey, presentedSetsArray);

        presentation.ClientMetadata.IfSome(meta => result.Add(ClientMetadataJsonKey, JObject.FromObject(meta)));
        presentation.Name.IfSome(name => result.Add(NameJsonKey, name));
        result.Add(LastTimeUsedJsonKey, presentation.LastTimeUsed);

        return result;
    }

    public static string Serialize(this CompletedPresentation presentation) => presentation.EncodeToJson().ToString();

    public static CompletedPresentation Deserialize(string json)
    {
        var jObject = JObject.Parse(json);
        return DecodeFromJson(jObject);
    }

    public static CompletedPresentation DecodeFromJson(JObject jObject)
    {
        var presentationId = jObject[PresentationIdJsonKey]!.ToString();
        var clientId = jObject[ClientIdJsonKey]!.ToString();

        var setsToken = jObject[PresentedCredentialSetsJsonKey]!;
        var setsArray = setsToken.ToObject<JArray>()!;
        var presentedSets = PresentedCredentialSetExtensions.DecodeFromJson(setsArray);

        var clientMetadata =
            from token in jObject.GetByKey(ClientMetadataJsonKey).ToOption()
            select token.ToObject<ClientMetadata>()!;

        var name =
            from token in jObject.GetByKey(NameJsonKey).ToOption()
            select token.ToObject<string>()!;

        var lastTimeUsed = jObject[LastTimeUsedJsonKey]!.ToObject<DateTimeOffset>();

        return new CompletedPresentation(
            presentationId,
            clientId,
            presentedSets,
            clientMetadata,
            name,
            lastTimeUsed);
    }
}
