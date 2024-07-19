using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models.SdJwt;

public record SdJwtCredentialRequest
{
    public CredentialRequest VciRequest { get; }
    
    /// <summary>
    ///     Gets the verifiable credential type (vct).
    /// </summary>
    public Vct Vct { get; }
    
    internal SdJwtCredentialRequest(CredentialRequest vciRequest, Vct vct)
    {
        VciRequest = vciRequest;
        Vct = vct;
    }
}

public static class SdJwtCredentialRequestFun
{
    public static string EncodeToJson(this SdJwtCredentialRequest request)
    {
        var json = request.VciRequest.EncodeToJson();

        json.Add("vct", request.Vct.ToString());
        
        return json.ToString();
    }
}
