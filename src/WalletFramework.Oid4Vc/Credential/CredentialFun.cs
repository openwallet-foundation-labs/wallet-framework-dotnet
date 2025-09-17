using OneOf;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.Credential;

public static class CredentialFun
{
    public static OneOf<Vct, DocType> GetCredentialType(this ICredential credential)
    {
        switch (credential)
        {
            case SdJwtRecord sdJwt:
                return Vct.ValidVct(sdJwt.Vct).UnwrapOrThrow();
            case MdocRecord mdoc:
                return mdoc.DocType;
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
