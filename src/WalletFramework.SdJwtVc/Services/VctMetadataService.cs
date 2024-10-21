using LanguageExt;
using Microsoft.Extensions.Logging;
using WalletFramework.Core.Functional;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.VctMetadata;
using static WalletFramework.Core.Json.JsonFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadata;

namespace WalletFramework.SdJwtVc.Services;

public class VctMetadataService : IVctMetadataService
{
    private readonly ILogger<VctMetadataService> _logger;
    private readonly HttpClient _httpClient;
    
    public VctMetadataService(
        IHttpClientFactory httpClientFactory, 
        ILogger<VctMetadataService> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public async Task<Option<VctMetadata>> ProcessMetadata(Vct vct)
    {
        if(!Uri.TryCreate(vct, UriKind.Absolute, out Uri vctUri))
            return Option<VctMetadata>.None;

        var baseEndpoint = new Uri(vctUri.GetLeftPart(UriPartial.Authority));
        var credentialName = vctUri.AbsolutePath;
        
        var metadataUrl = new Uri(baseEndpoint, $".well-known/vct{credentialName}");

        try
        {
            var response = await _httpClient.GetAsync(metadataUrl);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return ParseAsJObject(str).OnSuccess(ValidVctMetadata).Match(
                    validMetadata => validMetadata,
                    _ => Option<VctMetadata>.None);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e, 
                "Could not retrieve vct metadata by vct: {Vct}", vct);
        }
        
        return Option<VctMetadata>.None;
    }
}
