using LanguageExt;
using WalletFramework.MdocLib.Elements;

namespace WalletFramework.MdocLib.Security;

public record KeyAuthorizations(
    Option<List<NameSpace>> AuthorizedNameSpaces,
    Option<Dictionary<NameSpace, List<ElementIdentifier>>> AuthorizedDataElements);
