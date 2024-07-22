using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using static WalletFramework.Core.Json.JsonFun;
using static WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models.IssuerMetadata;

namespace WalletFramework.Oid4Vc.Oid4Vci.Issuer.Implementations;

public class IssuerMetadataService : IIssuerMetadataService
{
    public IssuerMetadataService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    private readonly HttpClient _httpClient;
    
    public async Task<Validation<IssuerMetadata>> ProcessMetadata(Uri issuerEndpoint, Locale language)
    {
        var baseEndpoint = issuerEndpoint
            .AbsolutePath
            .EndsWith("/")
            ? issuerEndpoint
            : new Uri(issuerEndpoint.OriginalString + "/");
        
        var metadataUrl = new Uri(baseEndpoint, ".well-known/openid-credential-issuer");
        
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", language);
        
        var response = await _httpClient.GetAsync(metadataUrl);
        if (response.IsSuccessStatusCode)
        {
            var str = await response.Content.ReadAsStringAsync();
            return ParseAsJObject(str).OnSuccess(ValidIssuerMetadata);
        }
        
        throw new HttpRequestException($"Failed to get Issuer metadata. Status code is {response.StatusCode}");
    }
}
