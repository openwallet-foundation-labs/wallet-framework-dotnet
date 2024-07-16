using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.CredentialOffer;
using static WalletFramework.Oid4Vc.Tests.Oid4Vci.Samples.CredentialOfferSample;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredOffer;

public class CredentialOfferTests
{
    [Fact]
    public void Can_Parse_From_Json()
    {
        var sample = PreAuth;

        ValidCredentialOffer(sample).Match(
            offer =>
            {
                offer.CredentialIssuer.Should().Be(CredentialIssuer);

                var ids = offer.CredentialConfigurationIds.Select(id => (string)id).ToList();

                ids.Length().Should().Be(2);
                ids.Should().Contain(UniversityDegreeCredential);
                ids.Should().Contain(OrgIso1801351Mdl);

                offer.Grants.Match(
                    grants =>
                    {
                        grants.AuthorizationCode.IsNone.Should().BeTrue();
                        grants.PreAuthorizedCode.Match(
                            preAuthCode =>
                            {
                                preAuthCode.ToString().Should().Be(PreAuthorizedCode);
                                preAuthCode.TransactionCode.Match(
                                    transactionCode =>
                                    {
                                        transactionCode.Length.Match(
                                            length => length.Should().Be(Length),
                                            () => Assert.Fail("Length must be some"));

                                        transactionCode.Description.Match(
                                            description => description.Should().Be(Description),
                                            () => Assert.Fail("Description must be some"));

                                        transactionCode.InputMode.Match(
                                            mode => mode.ToString().Should().Be("numeric"),
                                            () => Assert.Fail("InputMode must be some"));
                                    },
                                    () => Assert.Fail("TransactionCode must be some"));
                            },
                            () => Assert.Fail("PreAuthorizedCode must be some"));
                    },
                    () => Assert.Fail("Grants must be some"));
            },
            _ => Assert.Fail("Offer must be valid"));
    }

    [Fact]
    public void Offer_With_Invalid_Structure_Is_Rejected()
    {
        var sample = PreAuth;

        sample["credential_issuer"] = "this is not a valid URI";
        sample["credential_configuration_ids"] = new JArray();
        
        sample["grants"]!["urn:ietf:params:oauth:grant-type:pre-authorized_code"]!["pre-authorized_code"] = null;

        ValidCredentialOffer(sample).Match(
            _ => Assert.Fail("Offer with invalid structure must be invalid"),
            errors =>
            {
                errors.Should().ContainSingle(error => error is CredentialIssuerError);
                errors.Should().ContainSingle(error => error is EnumerableIsEmptyError<CredentialConfigurationId>);
            }
        );
    }
}
