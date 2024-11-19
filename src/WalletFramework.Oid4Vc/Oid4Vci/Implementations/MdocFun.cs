using LanguageExt;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public static class MdocFun
{
    public static Option<List<MdocDisplay>> CreateMdocDisplays(MdocConfiguration configuration)
    {
        var claimsDisplays = 
            from claimsMetadata in configuration.Claims
            from dict in claimsMetadata.ToClaimsDisplays()
            select dict;

        var credentialConfigurationDisplay = configuration.CredentialConfiguration.Display;
        var result =
            from credentialDisplays in credentialConfigurationDisplay
            let mdocDisplays = credentialDisplays.Select(credentialDisplay =>
            {
                var logo = 
                    from credentialLogo in credentialDisplay.Logo
                    select new MdocLogo(credentialLogo.Uri);
        
                var mdocName = 
                    from credentialName in credentialDisplay.Name
                    from name in MdocName.OptionMdocName(credentialName.ToString())
                    select name;
        
                var backgroundColor = credentialDisplay.BackgroundColor;
                var textColor = credentialDisplay.TextColor;
                var locale = credentialDisplay.Locale;
        
                return new MdocDisplay(logo, mdocName, backgroundColor, textColor, locale, claimsDisplays);
            }).ToList()
            select mdocDisplays;

        return result;
    }
}
