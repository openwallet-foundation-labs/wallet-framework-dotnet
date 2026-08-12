using System.IO.Compression;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using WalletFramework.Core.Credentials;

namespace WalletFramework.Core.StatusList;

public class StatusListService(
    IHttpClientFactory httpClientFactory,
    ILogger<StatusListService> logger) : IStatusListService
{
    public async Task<Option<CredentialState>> GetState(StatusListEntry statusListEntry)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(statusListEntry.Uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return StatusListStateReader.GetState(content, statusListEntry.Idx);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not retrieve credential status from status list: {Uri}",
                statusListEntry.Uri);
        }
        
        return Option<CredentialState>.None;
    }
}
