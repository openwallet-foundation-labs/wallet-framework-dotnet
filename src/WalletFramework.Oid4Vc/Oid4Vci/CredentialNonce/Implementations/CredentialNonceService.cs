using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Implementations;

public class CredentialNonceService(IHttpClientFactory httpClientFactory) : ICredentialNonceService
{
    public async Task<Models.CredentialNonce> GetCredentialNonce(CredentialNonceEndpoint credentialNonceEndpoint)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(credentialNonceEndpoint.Value, new StringContent(""));

        var message = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Requesting the c_nonce failed. Status Code is {response.StatusCode} with message: {message}");
        
        return (from jToken in JObject.Parse(message).GetByKey("c_nonce") 
                from docType in Models.CredentialNonce.ValidCredentialNonce(jToken.ToString())
                select docType)
            .Match(
                nonce => nonce, 
                _ => throw new InvalidOperationException("Failed deserialize c_nonce from nonce endpoint response"));
    }
}
