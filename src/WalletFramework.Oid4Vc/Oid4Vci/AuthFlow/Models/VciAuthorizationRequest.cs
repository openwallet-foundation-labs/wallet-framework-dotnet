using System.Web;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

internal record VciAuthorizationRequest
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
    
    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string AuthFlowSessionState { get; }
        
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

    public VciAuthorizationRequest(
        AuthFlowSessionState authFlowSessionState,
        ClientOptions clientOptions,
        AuthorizationCodeParameters authorizationCodeParameters,
        AuthorizationDetails[]? authorizationDetails, 
        string? scope, 
        string? issuerState, 
        string? userHint,  
        string? resource)
    {
        ClientId = clientOptions.ClientId;
        RedirectUri = clientOptions.RedirectUri;
        WalletIssuer = clientOptions.WalletIssuer;
        CodeChallenge = authorizationCodeParameters.Challenge;
        CodeChallengeMethod = authorizationCodeParameters.CodeChallengeMethod;
        AuthFlowSessionState = authFlowSessionState;
        AuthorizationDetails = authorizationDetails;
        IssuerState = issuerState;
        UserHint = userHint;
        Scope = scope;
        Resource = resource;
    }
}

internal static class VciAuthorizationRequestExtensions
{
    internal static FormUrlEncodedContent ToFormUrlEncoded(this VciAuthorizationRequest authorizationRequest)
    {
        var keyValuePairs = new List<KeyValuePair<string, string>>();
        
        if (!string.IsNullOrEmpty(authorizationRequest.ClientId))
            keyValuePairs.Add(new KeyValuePair<string, string>("client_id", authorizationRequest.ClientId));
        
        if (!string.IsNullOrEmpty(authorizationRequest.ResponseType))
            keyValuePairs.Add(new KeyValuePair<string, string>("response_type", authorizationRequest.ResponseType));
        
        if (!string.IsNullOrEmpty(authorizationRequest.RedirectUri))
            keyValuePairs.Add(new KeyValuePair<string, string>("redirect_uri", authorizationRequest.RedirectUri));
        
        if (!string.IsNullOrEmpty(authorizationRequest.CodeChallenge))
            keyValuePairs.Add(new KeyValuePair<string, string>("code_challenge", authorizationRequest.CodeChallenge));
            
        if (!string.IsNullOrEmpty(authorizationRequest.CodeChallengeMethod))
            keyValuePairs.Add(new KeyValuePair<string, string>("code_challenge_method", authorizationRequest.CodeChallengeMethod));
            
        if (!string.IsNullOrEmpty(authorizationRequest.AuthFlowSessionState))
            keyValuePairs.Add(new KeyValuePair<string, string>("state", authorizationRequest.AuthFlowSessionState));
        
        if (authorizationRequest.AuthorizationDetails != null)
            keyValuePairs.Add(new KeyValuePair<string, string>("authorization_details", SerializeObject(authorizationRequest.AuthorizationDetails)));
            
        if (!string.IsNullOrEmpty(authorizationRequest.IssuerState))
            keyValuePairs.Add(new KeyValuePair<string, string>("issuer_state", authorizationRequest.IssuerState));
            
        if (!string.IsNullOrEmpty(authorizationRequest.WalletIssuer))
            keyValuePairs.Add(new KeyValuePair<string, string>("wallet_issuer", authorizationRequest.WalletIssuer));
            
        if (!string.IsNullOrEmpty(authorizationRequest.UserHint))
            keyValuePairs.Add(new KeyValuePair<string, string>("user_hint", authorizationRequest.UserHint));
            
        if (!string.IsNullOrEmpty(authorizationRequest.Scope))
            keyValuePairs.Add(new KeyValuePair<string, string>("scope", authorizationRequest.Scope));
        
        return new FormUrlEncodedContent(keyValuePairs);
    }
    
    internal static string ToQueryString(this VciAuthorizationRequest authorizationRequest)
    {
        var queryString = "?";
        
        if (!string.IsNullOrEmpty(authorizationRequest.ClientId))
            queryString = queryString + "client_id=" + authorizationRequest.ClientId + "&";
        
        if (!string.IsNullOrEmpty(authorizationRequest.ResponseType))
            queryString = queryString + "response_type=" + authorizationRequest.ResponseType + "&";
        
        if (!string.IsNullOrEmpty(authorizationRequest.RedirectUri))
            queryString = queryString + "redirect_uri=" + HttpUtility.UrlEncode(authorizationRequest.RedirectUri) + "&";
        
        if (!string.IsNullOrEmpty(authorizationRequest.CodeChallenge))
            queryString = queryString + "code_challenge=" + authorizationRequest.CodeChallenge + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.CodeChallengeMethod))
            queryString = queryString + "code_challenge_method=" + authorizationRequest.CodeChallengeMethod + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.AuthFlowSessionState))
            queryString = queryString + "state=" + authorizationRequest.AuthFlowSessionState + "&";
        
        if (authorizationRequest.AuthorizationDetails != null)
            queryString = queryString + "authorization_details=" + HttpUtility.UrlEncode(SerializeObject(authorizationRequest.AuthorizationDetails)) + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.IssuerState))
            queryString = queryString + "issuer_state=" + authorizationRequest.IssuerState + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.WalletIssuer))
            queryString = queryString + "wallet_issuer=" + HttpUtility.UrlEncode(authorizationRequest.WalletIssuer) + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.UserHint))
            queryString = queryString + "user_hint=" + authorizationRequest.UserHint + "&";
            
        if (!string.IsNullOrEmpty(authorizationRequest.Scope))
            queryString = queryString + "scope=" + authorizationRequest.Scope + "&";

        return queryString.TrimEnd('&');
    }
}
