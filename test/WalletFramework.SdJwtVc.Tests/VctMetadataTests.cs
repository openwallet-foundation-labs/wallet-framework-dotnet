using LanguageExt.UnsafeValueAccess;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models.VctMetadata;
using WalletFramework.SdJwtVc.Models.VctMetadata.Claims;
using WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

namespace WalletFramework.SdJwtVc.Tests;

public class VctMetadataTests
{
    private readonly string _metadataJson;

    public VctMetadataTests()
    {
        _metadataJson =
            "{\n  \"vct\":\"https://betelgeuse.example.com/education_credential\",\n  \"name\":\"Betelgeuse Education Credential - Preliminary Version\",\n  \"description\":\"This is our development version of the education credential. Don't panic.\",\n  \"extends\":\"https://galaxy.example.com/galactic-education-credential-0.9\",\n  \"extends#integrity\":\"sha256-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-WRL5\",\n  \"display\":{\n      \"en-US\":{\n        \"name\":\"Betelgeuse Education Credential\",\n        \"rendering\":{\n          \"simple\":{\n            \"logo\":{\n              \"uri\":\"https://betelgeuse.example.com/public/education-logo.png\",\n              \"uri#integrity\":\"sha256-LmXfh-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1V\",\n              \"alt_text\":\"Betelgeuse Ministry of Education logo\"\n            },\n            \"background_color\":\"#12107c\",\n            \"text_color\":\"#FFFFFF\"\n          },\n          \"svg_templates\":[\n            {\n              \"uri\":\"https://betelgeuse.example.com/public/credential-english.svg\",\n              \"uri#integrity\":\"sha256-8cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-9c\",\n              \"properties\":{\n                \"orientation\":\"landscape\",\n                \"color_scheme\":\"light\",\n                \"contrast\":\"high\"\n              }\n            }\n          ]\n        }\n      },\n      \"de-DE\":{\n        \"name\":\"Betelgeuse-Bildungsnachweis\",\n        \"rendering\":{\n          \"simple\":{\n            \"logo\":{\n              \"uri\":\"https://betelgeuse.example.com/public/education-logo-de.png\",\n              \"uri#integrity\":\"sha256-LmXfh-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1V\",\n              \"alt_text\":\"Logo des Betelgeusischen Bildungsministeriums\"\n            },\n            \"background_color\":\"#12107c\",\n            \"text_color\":\"#FFFFFF\"\n          },\n          \"svg_templates\":[\n            {\n              \"uri\":\"https://betelgeuse.example.com/public/credential-german.svg\",\n              \"uri#integrity\":\"sha256-8cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-9c\",\n              \"properties\":{\n                \"orientation\":\"landscape\",\n                \"color_scheme\":\"light\",\n                \"contrast\":\"high\"\n              }\n            }\n          ]\n        }\n      }\n    },\n  \"claims\":[\n    {\n      \"path\":[\"name\"],\n      \"display\":{\n        \"de-DE\":{\n          \"label\":\"Vor- und Nachname\",\n          \"description\":\"Der Name des Studenten\"\n        },\n        \"en-US\":{\n          \"label\":\"Name\",\n          \"description\":\"The name of the student\"\n        }\n      },\n      \"verification\":\"verified\",\n      \"sd\":\"allowed\"\n    },\n    {\n      \"path\":[\"address\"],\n      \"display\":{\n        \"de-DE\":{\n          \"label\":\"Adresse\",\n          \"description\":\"Adresse zum Zeitpunkt des Abschlusses\"\n        },\n        \"en-US\":{\n          \"label\":\"Address\",\n          \"description\":\"Address at the time of graduation\"\n        }\n      },\n      \"verification\":\"self-attested\",\n      \"sd\":\"always\"\n    },\n    {\n      \"path\":[\"address\", \"street_address\"],\n      \"display\":{\n        \"de-DE\":{\n          \"label\":\"Straße\"\n        },\n        \"en-US\":{\n          \"label\":\"Street Address\"\n        }\n      },\n      \"verification\":\"self-attested\",\n      \"sd\":\"always\"\n    },\n    {\n      \"path\":[\"degrees\", null],\n      \"display\":{\n        \"de-DE\":{\n          \"label\":\"Abschluss\",\n          \"description\":\"Der Abschluss des Studenten\"\n        },\n        \"en-US\":{\n          \"label\":\"Degree\",\n          \"description\":\"Degree earned by the student\"\n        }\n      },\n      \"verification\":\"authoritative\",\n      \"sd\":\"allowed\"\n    }\n  ],\n  \"schema_url\":\"https://exampleuniversity.com/public/credential-schema-0.9\",\n  \"schema_url#integrity\":\"sha256-o984vn819a48ui1llkwPmKjZ5t0WRL5ca_xGgX3c1VLmXfh\"\n}";
    }
    
    [Fact]
    public void Can_Create_VctMetadata()
    {
        // Arrange
        var localeEnUs = Locale.ValidLocale("en-US").UnwrapOrThrow();
        var localeDeDe = Locale.ValidLocale("de-DE").UnwrapOrThrow();
        
        // Act
        var result = VctMetadata.ValidVctMetadata(JObject.Parse(_metadataJson));

        // Assert
        Assert.True(result.IsSuccess);

        var vctMetadata = result.UnwrapOrThrow();
        Assert.Equal("https://betelgeuse.example.com/education_credential", vctMetadata.Vct);
        Assert.Equal("Betelgeuse Education Credential - Preliminary Version", vctMetadata.Name);
        Assert.Equal("This is our development version of the education credential. Don't panic.", vctMetadata.Description);
        
        Assert.True(vctMetadata.Extends.IsSome);
        var extends = vctMetadata.Extends.ValueUnsafe();
        Assert.Equal("https://galaxy.example.com/galactic-education-credential-0.9", extends.Uri.ToString());
        Assert.True(extends.Integrity.IsSome);
        Assert.Equal("sha256-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-WRL5", extends.Integrity.ValueUnsafe());
        
        Assert.True(vctMetadata.Schema.IsNone);
        
        Assert.True(vctMetadata.SchemaUrl.IsSome);
        var schemaUrl = vctMetadata.SchemaUrl.ValueUnsafe();
        Assert.Equal("https://exampleuniversity.com/public/credential-schema-0.9", schemaUrl.Uri.ToString());
        Assert.True(schemaUrl.Integrity.IsSome);
        Assert.Equal("sha256-o984vn819a48ui1llkwPmKjZ5t0WRL5ca_xGgX3c1VLmXfh", schemaUrl.Integrity.ValueUnsafe());

        Assert.True(vctMetadata.Display.IsSome);
        var display = vctMetadata.Display.ValueUnsafe();
        
        Assert.Equal(2, display.Count);
        
        Assert.True(display.ContainsKey(localeEnUs));
        Assert.Equal("Betelgeuse Education Credential", display[localeEnUs].Name);
        Assert.True(display[localeEnUs].Rendering.IsSome);
        var renderingEnUs = display[localeEnUs].Rendering.ValueUnsafe();
        
        Assert.True(renderingEnUs.Simple.IsSome);
        var simpleEnUs = renderingEnUs.Simple.ValueUnsafe();
        
        Assert.True(simpleEnUs.Logo.IsSome);
        var logoEnUs = simpleEnUs.Logo.ValueUnsafe();
        Assert.True(logoEnUs.Uri.IsSome);
        var logoUriEnUs = logoEnUs.Uri.ValueUnsafe();
        Assert.Equal("https://betelgeuse.example.com/public/education-logo.png", logoUriEnUs.Uri.ToString());
        Assert.True(logoUriEnUs.Integrity.IsSome);
        Assert.Equal("sha256-LmXfh-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1V", logoUriEnUs.Integrity.ValueUnsafe());
        
        Assert.Equal("Betelgeuse Ministry of Education logo", logoEnUs.AltText);
        Assert.True(simpleEnUs.BackgroundColor.IsSome);
        Assert.Equal("#12107C", simpleEnUs.BackgroundColor.ValueUnsafe());
        Assert.True(simpleEnUs.TextColor.IsSome);
        Assert.Equal("#FFFFFF", simpleEnUs.TextColor.ValueUnsafe());
        
        Assert.True(renderingEnUs.SvgTemplates.IsSome);
        var svgTemplatesEnUs = renderingEnUs.SvgTemplates.ValueUnsafe();
        Assert.Single(svgTemplatesEnUs);

        var svgTemplateEnUs = svgTemplatesEnUs.Single();
        var svgTemplateUriEnUs = svgTemplateEnUs.Uri;
        Assert.Equal("https://betelgeuse.example.com/public/credential-english.svg", svgTemplateUriEnUs.Uri.ToString());
        Assert.True(svgTemplateUriEnUs.Integrity.IsSome);
        Assert.Equal("sha256-8cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-9c", svgTemplateUriEnUs.Integrity.ValueUnsafe());
        
        Assert.True(svgTemplateEnUs.Properties.IsSome);
        var svgTemplatePropertiesEnUs = svgTemplateEnUs.Properties.ValueUnsafe();
        Assert.True(svgTemplatePropertiesEnUs.Orientation.IsSome);
        Assert.Equal(SvgTemplateOrientation.Landscape, svgTemplatePropertiesEnUs.Orientation.ValueUnsafe());
        Assert.True(svgTemplatePropertiesEnUs.ColorScheme.IsSome);
        Assert.Equal(SvgTemplateColorScheme.Light, svgTemplatePropertiesEnUs.ColorScheme.ValueUnsafe());
        Assert.True(svgTemplatePropertiesEnUs.Contrast.IsSome);
        Assert.Equal(SvgTemplateContrast.High, svgTemplatePropertiesEnUs.Contrast.ValueUnsafe());
        
        Assert.True(display.ContainsKey(localeDeDe));
        Assert.Equal("Betelgeuse-Bildungsnachweis", display[localeDeDe].Name);
        Assert.True(display[localeDeDe].Rendering.IsSome);
        var renderingDeDe = display[localeDeDe].Rendering.ValueUnsafe();
        
        Assert.True(renderingDeDe.Simple.IsSome);
        var simpleDeDe = renderingDeDe.Simple.ValueUnsafe();
        
        Assert.True(simpleDeDe.Logo.IsSome);
        var logoDeDe = simpleDeDe.Logo.ValueUnsafe();
        Assert.True(logoDeDe.Uri.IsSome);
        var logoUriDeDe = logoDeDe.Uri.ValueUnsafe();
        Assert.Equal("https://betelgeuse.example.com/public/education-logo-de.png", logoUriDeDe.Uri.ToString());
        Assert.True(logoUriDeDe.Integrity.IsSome);
        Assert.Equal("sha256-LmXfh-9cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1V", logoUriDeDe.Integrity.ValueUnsafe());
        
        Assert.Equal("Logo des Betelgeusischen Bildungsministeriums", logoDeDe.AltText);
        Assert.True(simpleDeDe.BackgroundColor.IsSome);
        Assert.Equal("#12107C", simpleDeDe.BackgroundColor.ValueUnsafe());
        Assert.True(simpleDeDe.TextColor.IsSome);
        Assert.Equal("#FFFFFF", simpleDeDe.TextColor.ValueUnsafe());
        
        Assert.True(renderingDeDe.SvgTemplates.IsSome);
        var svgTemplatesDeDe = renderingDeDe.SvgTemplates.ValueUnsafe();
        Assert.Single(svgTemplatesDeDe);

        var svgTemplateDeDe = svgTemplatesDeDe.Single();
        var svgTemplateUriDeDe = svgTemplateDeDe.Uri;
        Assert.Equal("https://betelgeuse.example.com/public/credential-german.svg", svgTemplateUriDeDe.Uri.ToString());
        Assert.True(svgTemplateUriDeDe.Integrity.IsSome);
        Assert.Equal("sha256-8cLlJNXN-TsMk-PmKjZ5t0WRL5ca_xGgX3c1VLmXfh-9c", svgTemplateUriDeDe.Integrity.ValueUnsafe());
        
        Assert.True(svgTemplateDeDe.Properties.IsSome);
        var svgTemplatePropertiesDeDe = svgTemplateDeDe.Properties.ValueUnsafe();
        Assert.True(svgTemplatePropertiesDeDe.Orientation.IsSome);
        Assert.Equal(SvgTemplateOrientation.Landscape, svgTemplatePropertiesDeDe.Orientation.ValueUnsafe());
        Assert.True(svgTemplatePropertiesDeDe.ColorScheme.IsSome);
        Assert.Equal(SvgTemplateColorScheme.Light, svgTemplatePropertiesDeDe.ColorScheme.ValueUnsafe());
        Assert.True(svgTemplatePropertiesDeDe.Contrast.IsSome);
        Assert.Equal(SvgTemplateContrast.High, svgTemplatePropertiesDeDe.Contrast.ValueUnsafe());
        
        Assert.True(vctMetadata.Claims.IsSome);
        var claims = vctMetadata.Claims.ValueUnsafe();
        Assert.Equal(4, claims.Length);

        var nameClaim = claims[0];
        Assert.Single((string?[])nameClaim.Path);
        Assert.Equal("name", ((string?[])nameClaim.Path).Single());
        
        Assert.True(nameClaim.Display.IsSome);
        var nameClaimDisplay = nameClaim.Display.ValueUnsafe();
        Assert.Equal(2, nameClaimDisplay.Count);
        
        Assert.True(nameClaimDisplay.ContainsKey(localeDeDe));
        var nameClaimDisplayDeDe = nameClaimDisplay[localeDeDe];
        Assert.True(nameClaimDisplayDeDe.Label.IsSome);
        Assert.Equal("Vor- und Nachname", nameClaimDisplayDeDe.Label.ValueUnsafe());
        
        Assert.True(nameClaimDisplayDeDe.Description.IsSome);
        Assert.Equal("Der Name des Studenten", nameClaimDisplayDeDe.Description.ValueUnsafe());
        
        Assert.True(nameClaimDisplay.ContainsKey(localeEnUs));
        var nameClaimDisplayEnUs = nameClaimDisplay[localeEnUs];
        Assert.True(nameClaimDisplayEnUs.Label.IsSome);
        Assert.Equal("Name", nameClaimDisplayEnUs.Label.ValueUnsafe());
        
        Assert.True(nameClaimDisplayEnUs.Description.IsSome);
        Assert.Equal("The name of the student", nameClaimDisplayEnUs.Description.ValueUnsafe());

        Assert.True(nameClaim.Verification.IsSome);
        Assert.Equal(ClaimVerification.Verified, nameClaim.Verification.ValueUnsafe());
        
        Assert.True(nameClaim.SelectiveDisclosure.IsSome);
        Assert.Equal(ClaimSelectiveDisclosure.Allowed, nameClaim.SelectiveDisclosure.ValueUnsafe());

        var addressClaim = claims[1];
        Assert.Single((string?[])addressClaim.Path);
        Assert.Equal("address", ((string?[])addressClaim.Path).Single());
        
        Assert.True(addressClaim.Display.IsSome);
        var addressClaimDisplay = addressClaim.Display.ValueUnsafe();
        Assert.Equal(2, addressClaimDisplay.Count);
        
        Assert.True(addressClaimDisplay.ContainsKey(localeDeDe));
        var addressClaimDisplayDeDe = addressClaimDisplay[localeDeDe];
        Assert.True(addressClaimDisplayDeDe.Label.IsSome);
        Assert.Equal("Adresse", addressClaimDisplayDeDe.Label.ValueUnsafe());
        
        Assert.True(addressClaimDisplayDeDe.Description.IsSome);
        Assert.Equal("Adresse zum Zeitpunkt des Abschlusses", addressClaimDisplayDeDe.Description.ValueUnsafe());
        
        Assert.True(addressClaimDisplay.ContainsKey(localeEnUs));
        var addressClaimDisplayEnUs = addressClaimDisplay[localeEnUs];
        Assert.True(addressClaimDisplayEnUs.Label.IsSome);
        Assert.Equal("Address", addressClaimDisplayEnUs.Label.ValueUnsafe());
        
        Assert.True(addressClaimDisplayEnUs.Description.IsSome);
        Assert.Equal("Address at the time of graduation", addressClaimDisplayEnUs.Description.ValueUnsafe());

        Assert.True(addressClaim.Verification.IsSome);
        Assert.Equal(ClaimVerification.SelfAttested, addressClaim.Verification.ValueUnsafe());
        
        Assert.True(addressClaim.SelectiveDisclosure.IsSome);
        Assert.Equal(ClaimSelectiveDisclosure.Always, addressClaim.SelectiveDisclosure.ValueUnsafe());
        
        var streetAddressClaim = claims[2];
        Assert.Equal(2, ((string?[])streetAddressClaim.Path).Length);
        Assert.Equal("address", ((string?[])streetAddressClaim.Path)[0]);
        Assert.Equal("street_address", ((string?[])streetAddressClaim.Path)[1]);
        
        Assert.True(streetAddressClaim.Display.IsSome);
        var streetAddressClaimDisplay = streetAddressClaim.Display.ValueUnsafe();
        Assert.Equal(2, streetAddressClaimDisplay.Count);
        
        Assert.True(streetAddressClaimDisplay.ContainsKey(localeDeDe));
        var streetAddressClaimDisplayDeDe = streetAddressClaimDisplay[localeDeDe];
        Assert.True(streetAddressClaimDisplayDeDe.Label.IsSome);
        Assert.Equal("Straße", streetAddressClaimDisplayDeDe.Label.ValueUnsafe());
        
        Assert.True(streetAddressClaimDisplayDeDe.Description.IsNone);
        
        Assert.True(streetAddressClaimDisplay.ContainsKey(localeEnUs));
        var streetAddressClaimDisplayEnUs = streetAddressClaimDisplay[localeEnUs];
        Assert.True(streetAddressClaimDisplayEnUs.Label.IsSome);
        Assert.Equal("Street Address", streetAddressClaimDisplayEnUs.Label.ValueUnsafe());
        
        Assert.True(streetAddressClaimDisplayEnUs.Description.IsNone);
        
        Assert.True(streetAddressClaim.Verification.IsSome);
        Assert.Equal(ClaimVerification.SelfAttested, streetAddressClaim.Verification.ValueUnsafe());
        
        Assert.True(streetAddressClaim.SelectiveDisclosure.IsSome);
        Assert.Equal(ClaimSelectiveDisclosure.Always, streetAddressClaim.SelectiveDisclosure.ValueUnsafe());
        
        var degreesClaim = claims[3];
        Assert.Equal(2, ((string?[])degreesClaim.Path).Length);
        Assert.Equal("degrees", ((string?[])degreesClaim.Path)[0]);
        Assert.Null(((string?[])degreesClaim.Path)[1]);
        
        Assert.True(degreesClaim.Display.IsSome);
        var degreesClaimDisplay = degreesClaim.Display.ValueUnsafe();
        Assert.Equal(2, degreesClaimDisplay.Count);
        
        Assert.True(degreesClaimDisplay.ContainsKey(localeDeDe));
        var degreesClaimDisplayDeDe = degreesClaimDisplay[localeDeDe];
        Assert.True(degreesClaimDisplayDeDe.Label.IsSome);
        Assert.Equal("Abschluss", degreesClaimDisplayDeDe.Label.ValueUnsafe());
        
        Assert.True(degreesClaimDisplayDeDe.Description.IsSome);
        Assert.Equal("Der Abschluss des Studenten", degreesClaimDisplayDeDe.Description.ValueUnsafe());
        
        Assert.True(degreesClaimDisplay.ContainsKey(localeEnUs));
        var degreesClaimDisplayEnUs = degreesClaimDisplay[localeEnUs];
        Assert.True(degreesClaimDisplayEnUs.Label.IsSome);
        Assert.Equal("Degree", degreesClaimDisplayEnUs.Label.ValueUnsafe());
        
        Assert.True(degreesClaimDisplayEnUs.Description.IsSome);
        Assert.Equal("Degree earned by the student", degreesClaimDisplayEnUs.Description.ValueUnsafe());

        Assert.True(degreesClaim.Verification.IsSome);
        Assert.Equal(ClaimVerification.Authoritative, degreesClaim.Verification.ValueUnsafe());
        
        Assert.True(degreesClaim.SelectiveDisclosure.IsSome);
        Assert.Equal(ClaimSelectiveDisclosure.Allowed, degreesClaim.SelectiveDisclosure.ValueUnsafe());
    }
}
