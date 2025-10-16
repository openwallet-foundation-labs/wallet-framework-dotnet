using LanguageExt;
using static LanguageExt.Prelude;
using WalletFramework.Core.Functional;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;

public sealed record AuthFlowSession(
    AuthFlowSessionState AuthFlowSessionState,
    AuthorizationData AuthorizationData,
    AuthorizationCodeParameters AuthorizationCodeParameters,
    Option<int> SpecVersion)
{
    public static AuthFlowSession Create(
        AuthorizationData authorizationData,
        string codeChallenge,
        string codeVerifier,
        AuthFlowSessionState authFlowSessionState,
        Option<int> specVersion)
    {
        var codeParameters = new AuthorizationCodeParameters(codeChallenge, codeVerifier);
        return new AuthFlowSession(authFlowSessionState, authorizationData, codeParameters, specVersion);
    }
}

public static class AuthFlowSessionJson
{
    private const string SessionStateJsonKey = "auth_flow_session_state";
    private const string AuthorizationDataJsonKey = "authorization_data";
    private const string AuthorizationCodeParametersJsonKey = "authorization_code_parameters";
    private const string SpecVersionJsonKey = "spec_version";

    public static JObject EncodeToJson(this AuthFlowSession session)
    {
        var result = new JObject
        {
            { SessionStateJsonKey, session.AuthFlowSessionState.AsString() },
            { AuthorizationDataJsonKey, session.AuthorizationData.EncodeToJson() },
            { AuthorizationCodeParametersJsonKey, JObject.FromObject(session.AuthorizationCodeParameters) }
        };

        session.SpecVersion.IfSome(version => result.Add(SpecVersionJsonKey, version));

        return result;
    }

    public static AuthFlowSession DecodeFromJson(JObject json)
    {
        var stateStr = json[SessionStateJsonKey]!.ToString();
        var state = AuthFlowSessionState.ValidAuthFlowSessionState(stateStr).UnwrapOrThrow();

        var authorizationData = AuthorizationDataFun.DecodeFromJson(json[AuthorizationDataJsonKey]!.ToObject<JObject>()!);
        var codeParams = json[AuthorizationCodeParametersJsonKey]!.ToObject<AuthorizationCodeParameters>()!;
        var specVersion = json[SpecVersionJsonKey]?.ToObject<int?>();

        var specOpt = specVersion.HasValue
            ? Some(specVersion.Value)
            : Option<int>.None;

        return new AuthFlowSession(state, authorizationData, codeParams, specOpt);
    }
}
