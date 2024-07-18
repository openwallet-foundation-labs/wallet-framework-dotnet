using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Localization.Samples;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicCurve;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicSuite;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.ElementDisplay;
using static WalletFramework.MdocLib.ElementIdentifier;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.ElementName;
using static WalletFramework.MdocLib.DocType;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;
using static WalletFramework.MdocLib.NameSpace;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredConfiguration.Mdoc.Samples;

public static class MdocConfigurationSample
{
    public const bool OneTimeUse = true;

    public const uint BatchSize = 10;

    public static CryptographicCurve CryptoCurve =>
        ValidCryptographicCurve(1).UnwrapOrThrow(new InvalidOperationException());

    public static CryptographicSuite CryptoSuite =>
        ValidCryptographicSuite("ES256").UnwrapOrThrow(new InvalidOperationException());

    public static DocType DocType =>
        ValidDoctype("org.iso.18013.5.1.mDL").UnwrapOrThrow(new InvalidOperationException());

    public static ElementDisplay EnglishDisplay => OptionalElementDisplay(EnglishDisplayJson).ToNullable() ??
                                                   throw new InvalidOperationException();

    public static ElementDisplay JapaneseDisplay => OptionalElementDisplay(JapaneseDisplayJson).ToNullable() ??
                                                    throw new InvalidOperationException();

    public static ElementIdentifier GivenName =>
        ValidElementIdentifier("given_name").UnwrapOrThrow(new InvalidOperationException());

    public static Format Format => ValidFormat("mso_mdoc").UnwrapOrThrow(new InvalidOperationException());

    public static JObject Valid => new()
    {
        ["format"] = Format.ToString(),
        ["doctype"] = DocType.ToString(),
        ["policy"] = new JObject
        {
            ["one_time_use"] = OneTimeUse,
            ["batch_size"] = BatchSize
        },
        ["cryptographic_suites_supported"] = new JArray { CryptoSuite.ToString() },
        ["cryptographic_curves_supported"] = new JArray { CryptoCurve.ToString() },
        ["claims"] = new JObject
        {
            [NameSpace] = new JObject
            {
                [GivenName] = new JObject
                {
                    ["display"] = new JArray
                    {
                        new JObject
                        {
                            ["name"] = EnglishName.ToString(),
                            ["locale"] = LocaleSample.English.ToString()
                        },
                        new JObject
                        {
                            ["name"] = JapaneseName.ToString(),
                            ["locale"] = LocaleSample.Japanese.ToString()
                        }
                    }
                }
            }
        }
    };

    public static NameSpace NameSpace =>
        ValidNameSpace("org.iso.18013.5.1").UnwrapOrThrow(new InvalidOperationException());

    private static ElementName EnglishName =>
        OptionalElementName("Given Name").ToNullable() ?? throw new InvalidOperationException();

    private static ElementName JapaneseName =>
        OptionalElementName("&#21517;&#21069;").ToNullable() ?? throw new InvalidOperationException();

    private static JObject EnglishDisplayJson => new()
    {
        ["name"] = EnglishName.ToString(),
        ["locale"] = LocaleSample.English.ToString()
    };

    private static JObject JapaneseDisplayJson => new()
    {
        ["name"] = JapaneseName.ToString(),
        ["locale"] = LocaleSample.Japanese.ToString()
    };
}
