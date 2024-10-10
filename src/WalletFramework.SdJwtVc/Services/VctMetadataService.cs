using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.VctMetadata;
using static WalletFramework.Core.Json.JsonFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadata;

namespace WalletFramework.SdJwtVc.Services;

public class VctMetadataService : IVctMetadataService
{
    private readonly HttpClient _httpClient;
    
    public VctMetadataService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public async Task<Validation<VctMetadata>> ProcessMetadata(Vct vct)
    {
        if(!Uri.TryCreate(vct, UriKind.Absolute, out Uri vctUri))
            return new UriCanNotBeParsedError<VctMetadata>();

        var baseEndpoint = new Uri(vctUri.GetLeftPart(UriPartial.Authority));
        var credentialName = vctUri.AbsolutePath;
        
        var metadataUrl = new Uri(baseEndpoint, $".well-known/vct{credentialName}");
        
        var response = await _httpClient.GetAsync(metadataUrl);
        if (response.IsSuccessStatusCode)
        {
            var str = await response.Content.ReadAsStringAsync();
            return ParseAsJObject(str).OnSuccess(ValidVctMetadata);
        }
        
        throw new HttpRequestException($"Failed to get VCT metadata for VCT '{vct}'. Status code is {response.StatusCode}");
    }
}
