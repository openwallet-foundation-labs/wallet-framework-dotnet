using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

internal record PushedAuthorizationRequest
{
    [JsonProperty("client_id")]
    public string ClientId { get; }
        
    [JsonProperty("response_type")]
    public string ResponseType { get; } = "code";

    [JsonProperty("redirect_uri")] 
    public string RedirectUri { get; }
        
    [JsonProperty("code_challenge")]
    public string CodeChallenge { get; }
        
    [JsonProperty("code_challenge_method")]
    public string CodeChallengeMethod { get; }
        
    [JsonProperty("authorization_details", NullValueHandling = NullValueHandling.Ignore)]
    public AuthorizationDetails[]? AuthorizationDetails { get; }
        
    [JsonProperty("issuer_state", NullValueHandling = NullValueHandling.Ignore)]
    public string? IssuerState { get; }
        
    [JsonProperty("wallet_issuer", NullValueHandling = NullValueHandling.Ignore)]
    public string? WalletIssuer { get; }
        
    [JsonProperty("user_hint", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserHint { get; }
        
    [JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
    public string? Scope { get; }
        
    [JsonProperty("resource", NullValueHandling = NullValueHandling.Ignore)]
    public string? Resource { get; }

    public PushedAuthorizationRequest(
        VciSessionId sessionId,
        ClientOptions clientOptions,
        AuthorizationCodeParameters authorizationCodeParameters,
        AuthorizationDetails[]? authorizationDetails, 
        string? scope, 
        string? issuerState, 
        string? userHint,  
        string? resource)
    {
        ClientId = clientOptions.ClientId;
        RedirectUri = clientOptions.RedirectUri + "?session=" + sessionId;
        WalletIssuer = clientOptions.WalletIssuer;
        CodeChallenge = authorizationCodeParameters.Challenge;
        CodeChallengeMethod = authorizationCodeParameters.CodeChallengeMethod;
        AuthorizationDetails = authorizationDetails;
        IssuerState = issuerState;
        UserHint = userHint;
        Scope = scope;
        Resource = resource;
    }
        
    public FormUrlEncodedContent ToFormUrlEncoded()
    {
        var keyValuePairs = new List<KeyValuePair<string, string>>();
        
        if (!string.IsNullOrEmpty(ClientId))
            keyValuePairs.Add(new KeyValuePair<string, string>("client_id", ClientId));
        
        if (!string.IsNullOrEmpty(ResponseType))
            keyValuePairs.Add(new KeyValuePair<string, string>("response_type", ResponseType));
        
        if (!string.IsNullOrEmpty(RedirectUri))
            keyValuePairs.Add(new KeyValuePair<string, string>("redirect_uri", RedirectUri));
        
        if (!string.IsNullOrEmpty(CodeChallenge))
            keyValuePairs.Add(new KeyValuePair<string, string>("code_challenge", CodeChallenge));
            
        if (!string.IsNullOrEmpty(CodeChallengeMethod))
            keyValuePairs.Add(new KeyValuePair<string, string>("code_challenge_method", CodeChallengeMethod));
            
        if (AuthorizationDetails != null)
            keyValuePairs.Add(new KeyValuePair<string, string>("authorization_details", SerializeObject(AuthorizationDetails)));
            
        if (!string.IsNullOrEmpty(IssuerState))
            keyValuePairs.Add(new KeyValuePair<string, string>("issuer_state", IssuerState));
            
        if (!string.IsNullOrEmpty(WalletIssuer))
            keyValuePairs.Add(new KeyValuePair<string, string>("wallet_issuer", WalletIssuer));
            
        if (!string.IsNullOrEmpty(UserHint))
            keyValuePairs.Add(new KeyValuePair<string, string>("user_hint", UserHint));
            
        if (!string.IsNullOrEmpty(Scope))
            keyValuePairs.Add(new KeyValuePair<string, string>("scope", Scope));
        
        return new FormUrlEncodedContent(keyValuePairs);
    }
}
