using System.Collections.Specialized;
using System.Net;
using System.Web;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using static WalletFramework.Core.Json.JsonFun;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.CredentialOffer;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Implementations;

public class CredentialOfferService : ICredentialOfferService
{
    public CredentialOfferService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    private readonly HttpClient _httpClient;
    
    public async Task<Validation<CredentialOffer>> ProcessCredentialOffer(Uri credentialOffer, Locale language)
    {
        NameValueCollection queryParams;
        try
        {
            queryParams = HttpUtility.ParseQueryString(credentialOffer.Query);
        }
        catch (Exception e)
        {
            return new CredentialOfferHasNoQueryParameterError(e);
        }

        if (queryParams["credential_offer"] is { } offer)
            return ParseAsJObject(offer).OnSuccess(ValidCredentialOffer);

        if (queryParams["credential_offer_uri"] is { } offerUri)
        {
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", language);
            var response = await _httpClient.GetAsync(offerUri);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                return ParseAsJObject(content).OnSuccess(ValidCredentialOffer);
            }

            return new CouldNotFetchCredentialOfferError(response.StatusCode);
        }

        return new CredentialOfferNotFoundError();
    }
}
