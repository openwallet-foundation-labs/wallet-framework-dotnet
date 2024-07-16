using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Tests;

public static class Samples
{
    public const string DocType = "org.iso.18013.5.1.mDL";

    public const string MdlNameSpace = "org.iso.18013.5.1";

    public static string EncodedMdoc =
        "omdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGxpc3N1ZXJTaWduZWSiamlzc3VlckF1dGiEQ6EBJqEYIVkBYTCCAV0wggEEoAMCAQICBgGMkdnCGTAKBggqhkjOPQQDAjA2MTQwMgYDVQQDDCtKMUZ3SlA4N0M2LVFOX1dTSU9tSkFRYzZuNUNRX2JaZGFGSjVHRG5XMVJrMB4XDTIzMTIyMjE0MDY1NloXDTI0MTAxNzE0MDY1NlowNjE0MDIGA1UEAwwrSjFGd0pQODdDNi1RTl9XU0lPbUpBUWM2bjVDUV9iWmRhRko1R0RuVzFSazBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABAKKVXm6CaWEcnMNWCpJETl3wqShCpfWNWDxYTho-MU4NlDrp8U8UiFZE_eKhVSLrtYZBKBNjcXpWaN_skxVDgwwCgYIKoZIzj0EAwIDRwAwRAIgZxY0nAXLxEZXi4zAruTW8SSpDbGv4EIZF03w5m6vk94CIFWs27FXUL2YJbnPZyWEMVpc10_Yun_sIFCsdCgmG49wWQHt2BhZAeilZ3ZlcnNpb25jMS4wb2RpZ2VzdEFsZ29yaXRobWdTSEEtMjU2bHZhbHVlRGlnZXN0c6Fxb3JnLmlzby4xODAxMy41LjGoAVggq5LwUJ4Jy8MzBmAR7O65W_4NixSl3Kkmn1psmuocCZcCWCC7sP7e-v42suDfOKC6dTMQoWpgDIbmwD59--YONHFnbgNYILY4GeGhkWGoTuzw9F916Py3l-un4eAX_Zfioy3O8RjoBFggEX-uX3dVHbW6aQh1IyJaoWZPknGzSfcflJaidasmgOsFWCAoO9XIxTfnwt7SfpORVvZzQFuFtIwnCmzC08s2JmtNHwZYIAVnMnACacLtBwxDCrvYUNCWY_GTTjfhxluHr-u3VVqBB1ggfAEdDf6xU-1yj5FxSG5hirqKK-6ONjImZAFtD852EUMIWCByNPYdiCSjGfBY_72Oo7_r4P53r0f1RbbGOkNauSeW8Gdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGx2YWxpZGl0eUluZm-jZnNpZ25lZMB0MjAyNC0wMS0xMlQwMDoxMDowNVppdmFsaWRGcm9twHQyMDI0LTAxLTEyVDAwOjEwOjA1Wmp2YWxpZFVudGlswHQyMDI1LTAxLTEyVDAwOjEwOjA1WlhAcXMRvT00XIWWPnfcUT_UH0jauS73QrnZqvrgh3ULI5RdZfOEg15V-glTVx_GAJPYJfXfr1wZ39NDFKOFXHxulmpuYW1lU3BhY2VzoXFvcmcuaXNvLjE4MDEzLjUuMYjYGFhbpGhkaWdlc3RJRAFmcmFuZG9tUHG55kyB7dP9e7fgHB5CmWxxZWxlbWVudElkZW50aWZpZXJqaXNzdWVfZGF0ZWxlbGVtZW50VmFsdWXZA-xqMjAyNC0wMS0xMtgYWFykaGRpZ2VzdElEAmZyYW5kb21QUcL8wVSWAXNqZYXe712cE3FlbGVtZW50SWRlbnRpZmllcmtleHBpcnlfZGF0ZWxlbGVtZW50VmFsdWXZA-xqMjAyNS0wMS0xMtgYWFqkaGRpZ2VzdElEA2ZyYW5kb21Q3LgYdsROkqsQwxAjmPR9wnFlbGVtZW50SWRlbnRpZmllcmtmYW1pbHlfbmFtZWxlbGVtZW50VmFsdWVrU2lsdmVyc3RvbmXYGFhSpGhkaWdlc3RJRARmcmFuZG9tUB7uRXveoCUB_jXjgL0riXRxZWxlbWVudElkZW50aWZpZXJqZ2l2ZW5fbmFtZWxlbGVtZW50VmFsdWVkSW5nYdgYWFukaGRpZ2VzdElEBWZyYW5kb21QyPuG9N0ftvZYxYVKGTBz9HFlbGVtZW50SWRlbnRpZmllcmpiaXJ0aF9kYXRlbGVsZW1lbnRWYWx1ZdkD7GoxOTkxLTExLTA22BhYVaRoZGlnZXN0SUQGZnJhbmRvbVAiZV6ZdFAMYYojfQcEpy3gcWVsZW1lbnRJZGVudGlmaWVyb2lzc3VpbmdfY291bnRyeWxlbGVtZW50VmFsdWViVVPYGFhbpGhkaWdlc3RJRAdmcmFuZG9tUG1s_4IFMcrUmse_xags6BBxZWxlbWVudElkZW50aWZpZXJvZG9jdW1lbnRfbnVtYmVybGVsZW1lbnRWYWx1ZWgxMjM0NTY3ONgYWKKkaGRpZ2VzdElECGZyYW5kb21QW0sDoPdZTLKbv3LQWtrqoHFlbGVtZW50SWRlbnRpZmllcnJkcml2aW5nX3ByaXZpbGVnZXNsZWxlbWVudFZhbHVlgaN1dmVoaWNsZV9jYXRlZ29yeV9jb2RlYUFqaXNzdWVfZGF0ZdkD7GoyMDIzLTAxLTAxa2V4cGlyeV9kYXRl2QPsajIwNDMtMDEtMDE";

    public static string SelectivelyDisclosedEncodedMdoc =
        "omdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGxpc3N1ZXJTaWduZWSiamlzc3VlckF1dGiEQ6EBJqEYIVkBYTCCAV0wggEEoAMCAQICBgGMkdnCGTAKBggqhkjOPQQDAjA2MTQwMgYDVQQDDCtKMUZ3SlA4N0M2LVFOX1dTSU9tSkFRYzZuNUNRX2JaZGFGSjVHRG5XMVJrMB4XDTIzMTIyMjE0MDY1NloXDTI0MTAxNzE0MDY1NlowNjE0MDIGA1UEAwwrSjFGd0pQODdDNi1RTl9XU0lPbUpBUWM2bjVDUV9iWmRhRko1R0RuVzFSazBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABAKKVXm6CaWEcnMNWCpJETl3wqShCpfWNWDxYTho-MU4NlDrp8U8UiFZE_eKhVSLrtYZBKBNjcXpWaN_skxVDgwwCgYIKoZIzj0EAwIDRwAwRAIgZxY0nAXLxEZXi4zAruTW8SSpDbGv4EIZF03w5m6vk94CIFWs27FXUL2YJbnPZyWEMVpc10_Yun_sIFCsdCgmG49wWQHt2BhZAeilZ3ZlcnNpb25jMS4wb2RpZ2VzdEFsZ29yaXRobWdTSEEtMjU2bHZhbHVlRGlnZXN0c6Fxb3JnLmlzby4xODAxMy41LjGoAVggq5LwUJ4Jy8MzBmAR7O65W_4NixSl3Kkmn1psmuocCZcCWCC7sP7e-v42suDfOKC6dTMQoWpgDIbmwD59--YONHFnbgNYILY4GeGhkWGoTuzw9F916Py3l-un4eAX_Zfioy3O8RjoBFggEX-uX3dVHbW6aQh1IyJaoWZPknGzSfcflJaidasmgOsFWCAoO9XIxTfnwt7SfpORVvZzQFuFtIwnCmzC08s2JmtNHwZYIAVnMnACacLtBwxDCrvYUNCWY_GTTjfhxluHr-u3VVqBB1ggfAEdDf6xU-1yj5FxSG5hirqKK-6ONjImZAFtD852EUMIWCByNPYdiCSjGfBY_72Oo7_r4P53r0f1RbbGOkNauSeW8Gdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGx2YWxpZGl0eUluZm-jZnNpZ25lZMB0MjAyNC0wMS0xMlQwMDoxMDowNVppdmFsaWRGcm9twHQyMDI0LTAxLTEyVDAwOjEwOjA1Wmp2YWxpZFVudGlswHQyMDI1LTAxLTEyVDAwOjEwOjA1WlhAcXMRvT00XIWWPnfcUT_UH0jauS73QrnZqvrgh3ULI5RdZfOEg15V-glTVx_GAJPYJfXfr1wZ39NDFKOFXHxulmpuYW1lU3BhY2VzoXFvcmcuaXNvLjE4MDEzLjUuMYPYGFhapGhkaWdlc3RJRANmcmFuZG9tUNy4GHbETpKrEMMQI5j0fcJxZWxlbWVudElkZW50aWZpZXJrZmFtaWx5X25hbWVsZWxlbWVudFZhbHVla1NpbHZlcnN0b25l2BhYUqRoZGlnZXN0SUQEZnJhbmRvbVAe7kV73qAlAf4144C9K4l0cWVsZW1lbnRJZGVudGlmaWVyamdpdmVuX25hbWVsZWxlbWVudFZhbHVlZEluZ2HYGFiipGhkaWdlc3RJRAhmcmFuZG9tUFtLA6D3WUyym79y0Fra6qBxZWxlbWVudElkZW50aWZpZXJyZHJpdmluZ19wcml2aWxlZ2VzbGVsZW1lbnRWYWx1ZYGjdXZlaGljbGVfY2F0ZWdvcnlfY29kZWFBamlzc3VlX2RhdGXZA-xqMjAyMy0wMS0wMWtleHBpcnlfZGF0ZdkD7GoyMDQzLTAxLTAx";

    public static ElementIdentifier GivenName =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("given_name")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );
    
    public static ElementIdentifier FamilyName =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("family_name")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );
    
    public static ElementIdentifier DrivingPrivileges =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("driving_privileges")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );

    public static CoseLabel Es256CoseLabel =>
        CoseLabel.ValidCoseLabel(CBORObject.FromObject(1)).Match(
            label => label,
            _ => throw new InvalidOperationException()
        );

    public static CoseLabel X509ChainCoseLabel =>
        CoseLabel.ValidCoseLabel(CBORObject.FromObject(33)).Match(
            label => label,
            _ => throw new InvalidOperationException()
        );

    public static ElementIdentifier ExpiryDateIdentifier =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("expiry_date")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );

    public static ElementIdentifier IssueDateIdentifier =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("issue_date")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );

    public static ElementIdentifier VehicleCategoryCodeIdentifier =>
        ElementIdentifier.ValidElementIdentifier(CBORObject.FromObject("vehicle_category_code")).Match(
            identifier => identifier,
            _ => throw new InvalidOperationException()
        );

    public static NameSpace MdlIsoNameSpace =>
        NameSpace.ValidNameSpace(CBORObject.FromObject(MdlNameSpace)).Match(
            space => space,
            _ => throw new InvalidOperationException()
        );
}
