using FluentAssertions;
using LanguageExt;
using Moq;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Encoding;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vp.Dcql;
using WalletFramework.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.Oid4Vp.Services;
using WalletFramework.Oid4Vp.Tests.Dcql.Samples;
using WalletFramework.Oid4Vp.Tests.Samples;
using WalletFramework.Oid4Vp.Tests.TS12SCA.Samples;
using WalletFramework.Oid4Vp.TransactionDatas;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vp.Tests.TS12SCA;

public class Ts12PaymentTransactionDataTests
{
    [Fact]
    public void Can_Parse_TS12_Payment_Transaction_Data()
    {
        var sample = Ts12PaymentTransactionDataSamples.GetBase64UrlStringSample();

        var sut = TransactionData.FromBase64Url(sample).UnwrapOrThrow();

        sut.IsT3.Should().BeTrue();
        var payment = sut.AsT3;
        payment.Payment.TransactionId.AsString.Should().Be("ts12-transaction-123");
        payment.Payment.DateTime.Match(
            dateTime => dateTime.AsString.Should().Be("2026-04-29T12:30:00Z"),
            () => Assert.Fail("Expected date_time to be present"));
        payment.Payment.Payee.Name.Should().Be("Merchant XYZ");
        payment.Payment.Payee.Id.Should().Be("merchant-xyz");
        payment.Payment.Payee.Logo.Match(
            logo => logo.Should().Be("https://merchant.example/logo.png"),
            () => Assert.Fail("Expected payee logo to be present"));
        payment.Payment.Payee.Website.Match(
            website => website.Should().Be("https://merchant.example"),
            () => Assert.Fail("Expected payee website to be present"));
        payment.Payment.Pisp.Match(
            pisp =>
            {
                pisp.LegalName.Should().Be("Payment Initiator Ltd");
                pisp.BrandName.Should().Be("PayFast");
                pisp.DomainName.Should().Be("payfast.example");
            },
            () => Assert.Fail("Expected pisp to be present"));
        payment.Payment.ExecutionDate.Match(
            executionDate => executionDate.AsString.Should().Be("2026-05-01"),
            () => Assert.Fail("Expected execution_date to be present"));
        payment.Payment.Currency.Should().Be("EUR");
        payment.Payment.Amount.Should().Be(23.58m);
        payment.Payment.AmountEstimated.Match(
            amountEstimated => amountEstimated.Should().BeTrue(),
            () => Assert.Fail("Expected amount_estimated to be present"));
        payment.Payment.AmountEarmarked.Match(
            amountEarmarked => amountEarmarked.Should().BeFalse(),
            () => Assert.Fail("Expected amount_earmarked to be present"));
        payment.Payment.SctInst.Match(
            sctInst => sctInst.Should().BeTrue(),
            () => Assert.Fail("Expected sct_inst to be present"));
        payment.Payment.Recurrence.Match(
            recurrence =>
            {
                recurrence.StartDate.Match(
                    startDate => startDate.AsString.Should().Be("2026-05-01"),
                    () => Assert.Fail("Expected recurrence start_date to be present"));
                recurrence.EndDate.Match(
                    endDate => endDate.AsString.Should().Be("2026-12-01"),
                    () => Assert.Fail("Expected recurrence end_date to be present"));
                recurrence.Number.Match(
                    number => number.Should().Be(8),
                    () => Assert.Fail("Expected recurrence number to be present"));
                recurrence.Frequency.Should().Be("MNTH");
                recurrence.MitOptions.Match(
                    mitOptions =>
                    {
                        mitOptions.AmountVariable.Match(
                            amountVariable => amountVariable.Should().BeTrue(),
                            () => Assert.Fail("Expected amount_variable to be present"));
                        mitOptions.MinAmount.Match(
                            minAmount => minAmount.Should().Be(10.01m),
                            () => Assert.Fail("Expected min_amount to be present"));
                        mitOptions.MaxAmount.Match(
                            maxAmount => maxAmount.Should().Be(100.99m),
                            () => Assert.Fail("Expected max_amount to be present"));
                        mitOptions.TotalAmount.Match(
                            totalAmount => totalAmount.Should().Be(800.50m),
                            () => Assert.Fail("Expected total_amount to be present"));
                        mitOptions.InitialAmount.Match(
                            initialAmount => initialAmount.Should().Be(5.25m),
                            () => Assert.Fail("Expected initial_amount to be present"));
                        mitOptions.InitialAmountNumber.Match(
                            initialAmountNumber => initialAmountNumber.Should().Be(2),
                            () => Assert.Fail("Expected initial_amount_number to be present"));
                        mitOptions.Apr.Match(
                            apr => apr.Should().Be(3.14m),
                            () => Assert.Fail("Expected apr to be present"));
                    },
                    () => Assert.Fail("Expected mit_options to be present"));
            },
                    () => Assert.Fail("Expected recurrence to be present"));
    }

    [Fact]
    public void TS12_Payment_Transaction_Data_Preserves_Original_Encoded_Value_For_Hashing()
    {
        var originalJson = Ts12PaymentTransactionDataSamples.JsonSample;
        var sample = Ts12PaymentTransactionDataSamples.GetBase64UrlString(originalJson);
        var reserializedJson = JObject.Parse(originalJson).ToString(Newtonsoft.Json.Formatting.None);
        var reserializedSample = Ts12PaymentTransactionDataSamples.GetBase64UrlString(reserializedJson);
        var expectedHash = Sha256Hash.ComputeHash(sample.AsByteArray).AsHex;
        var reserializedHash = Sha256Hash.ComputeHash(reserializedSample.AsByteArray).AsHex;

        var sut = TransactionData.FromBase64Url(sample).UnwrapOrThrow();

        sut.AsT3.TransactionDataProperties.Encoded.Should().Be(sample);
        sut.Hash(TransactionDataHashesAlg.Sha256).AsHex.Should().Be(expectedHash);
        sut.Hash(TransactionDataHashesAlg.Sha256).AsHex.Should().NotBe(reserializedHash);
    }

    [Fact]
    public void Fails_When_Required_TS12_Payment_Field_Is_Missing()
    {
        var json = JObject.Parse(Ts12PaymentTransactionDataSamples.JsonSample);
        ((JObject)json["payload"]!).Remove("amount");
        var sample = Ts12PaymentTransactionDataSamples.GetBase64UrlString(json.ToString());

        var sut = TransactionData.FromBase64Url(sample);

        sut.IsFail.Should().BeTrue();
    }

    [Fact]
    public void Optional_TS12_Booleans_Are_None_When_Absent()
    {
        var json = JObject.Parse(Ts12PaymentTransactionDataSamples.JsonSample);
        var payload = (JObject)json["payload"]!;
        payload.Remove("amount_estimated");
        payload.Remove("amount_earmarked");
        payload.Remove("sct_inst");
        var sample = Ts12PaymentTransactionDataSamples.GetBase64UrlString(json.ToString());

        var sut = TransactionData.FromBase64Url(sample).UnwrapOrThrow();

        var payment = sut.AsT3.Payment;
        payment.AmountEstimated.IsNone.Should().BeTrue();
        payment.AmountEarmarked.IsNone.Should().BeTrue();
        payment.SctInst.IsNone.Should().BeTrue();
    }

    [Fact]
    public void Authorization_Request_Accepts_TS12_Payment_Transaction_Data()
    {
        var authRequestJson = Ts12PaymentTransactionDataSamples.GetAuthRequestWithTs12PaymentTransactionDataStr(
            DcqlSamples.IdCardNationalitiesSecondIndexQueryJson);

        var sut = AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();

        var transactionData = sut.TransactionData.IfNone([]);
        transactionData.Should().ContainSingle();
        transactionData[0].IsT3.Should().BeTrue();
    }

    [Fact]
    public void TS12_Payment_Transaction_Data_Is_Attached_To_Credential_Candidate()
    {
        var authRequestJson = Ts12PaymentTransactionDataSamples.GetAuthRequestWithTs12PaymentTransactionDataStr(
            DcqlSamples.IdCardNationalitiesSecondIndexQueryJson);
        var authRequest = AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();
        var sdJwtRecord = SdJwtSamples.GetIdCardCredential();
        var credentials = new List<ICredential> { sdJwtRecord };
        var candidateQueryResult = authRequest.DcqlQuery.ProcessWith(credentials);
        var presentationRequest = new PresentationRequest(authRequest, candidateQueryResult);
        var transactionDatas = authRequest.TransactionData.IfNone([]);

        var result = TransactionDataFun.ProcessVpTransactionData(presentationRequest, transactionDatas);

        result.Match(
            updatedRequest =>
            {
                updatedRequest.CandidateQueryResult.Candidates.Match(
                    sets =>
                    {
                        sets.Should().ContainSingle();
                        sets[0].Candidates[0].TransactionData.Match(
                            data =>
                            {
                                data.Should().ContainSingle();
                                data[0].IsT3.Should().BeTrue();
                            },
                            () => Assert.Fail("Expected transaction data to be present"));
                    },
                    () => Assert.Fail("Expected candidate sets to be present"));
            },
            error => Assert.Fail($"Expected success but got error: {error}"));
    }

    [Fact]
    public async Task CreatePresentations_Includes_TS12_Transaction_Data_Hashes_For_Selected_SdJwt_Credential()
    {
        var authRequestJson = Ts12PaymentTransactionDataSamples.GetAuthRequestWithTs12PaymentTransactionDataStr(
            DcqlSamples.IdCardNationalitiesSecondIndexQueryJson);
        var authRequest = AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();
        var transactionData = authRequest.TransactionData.IfNone([]).Single();
        var credential = SdJwtSamples.GetIdCardCredential();
        var selectedCredential = new SelectedCredential(
            "idcard",
            credential,
            Option<List<ClaimQuery>>.None,
            new List<TransactionData> { transactionData });
        var expectedHash = transactionData.Hash(TransactionDataHashesAlg.Sha256).AsHex;
        Option<IEnumerable<string>> capturedHashes = Option<IEnumerable<string>>.None;
        Option<string> capturedHashesAlg = Option<string>.None;
        var sdJwtVcHolderService = new Mock<ISdJwtVcHolderService>();
        sdJwtVcHolderService
            .Setup(service => service.CreatePresentation(
                It.IsAny<SdJwtCredential>(),
                It.IsAny<ClaimPath[]>(),
                It.IsAny<Option<IEnumerable<string>>>(),
                It.IsAny<Option<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .Callback<SdJwtCredential, ClaimPath[], Option<IEnumerable<string>>, Option<string>, string?, string?>(
                (_, _, hashes, hashesAlg, _, _) =>
                {
                    capturedHashes = hashes;
                    capturedHashesAlg = hashesAlg;
                })
            .ReturnsAsync("presentation");
        var sut = new PresentationService(
            sdJwtVcHolderService.Object,
            Mock.Of<IMdocAuthenticationService>(),
            Mock.Of<IVerifierKeyService>());

        await sut.CreatePresentations(authRequest, [selectedCredential], Option<Origin>.None);

        capturedHashes.Match(
            hashes => hashes.ToList().Should().Equal(expectedHash),
            () => Assert.Fail("Expected transaction_data_hashes to be present"));
        capturedHashesAlg.Match(
            alg => alg.Should().Be("sha-256"),
            () => Assert.Fail("Expected transaction_data_hashes_alg to be present"));
    }
}
