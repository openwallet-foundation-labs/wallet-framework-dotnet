using LanguageExt;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models.Mdoc;

public record MdocCredentialRequest
{
    public CredentialRequest VciRequest { get; }
    
    public DocType DocType { get; }
    
    public Option<bool> NamespacedData { get; }

    public MdocCredentialRequest(CredentialRequest credentialRequest, MdocConfiguration configuration)
    {
        VciRequest = credentialRequest;
        DocType = configuration.DocType;
        
        // TODO: Decide if this should be true or false
        NamespacedData = false;
    }
}

public static class MdocCredentialRequestFun
{
    public static string AsJson(this MdocCredentialRequest request)
    {
        var json = request.VciRequest.EncodeToJson();
        
        json.Add("doctype", request.DocType.ToString());

        return json.ToString();
    }
}
