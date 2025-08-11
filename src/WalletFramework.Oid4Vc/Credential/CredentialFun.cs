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
        switch (credential)
        {
            case SdJwtCredential sdJwt:
                return sdJwt.Vct;
            // case MdocRecord mdoc:
            //     return mdoc.DocType;
            case MdocCredential mdoc:
                return mdoc.Mdoc.DocType;
            default:
                throw new InvalidOperationException("Invalid credential type");
        }
    }
    
    public static string GetCredentialTypeAsString(this ICredential credential)
    {
        return credential.GetCredentialType().Match(
            vct => vct,
            docType => docType.ToString()
        );
    }
}
