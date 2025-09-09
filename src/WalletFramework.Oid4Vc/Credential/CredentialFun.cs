using OneOf;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Credential;

public static class CredentialFun
{
    public static OneOf<Vct, DocType> GetCredentialType(this ICredential credential)
    {
        return credential switch
        {
            SdJwtCredential sdJwt => sdJwt.Vct,
            MdocCredential mdoc => mdoc.Mdoc.DocType,
            _ => throw new InvalidOperationException("Invalid credential type")
        };
    }
    
    public static string GetCredentialTypeAsString(this ICredential credential)
    {
        return credential.GetCredentialType().Match(
            vct => vct,
            docType => docType.ToString()
        );
    }
}
