// using FluentAssertions;
// using Hyperledger.Aries.Storage;
// using LanguageExt;
// using Newtonsoft.Json.Linq;
// using WalletFramework.Core.Credentials;
// using WalletFramework.Core.Cryptography.Models;
// using WalletFramework.Core.Functional;
// using WalletFramework.MdocLib;
// using WalletFramework.MdocLib.Tests;
// using WalletFramework.MdocVc.Display;
// using WalletFramework.MdocVc.Serialization;
// using Xunit;
//
// namespace WalletFramework.MdocVc.Tests;
//
// public class MdocRecordTests
// {
//     [Fact]
//     public void Can_Encode_To_Json()
//     {
//         var encodedMdoc = "ompuYW1lU3BhY2VzoXFvcmcuaXNvLjE4MDEzLjUuMYLYGFhSpGhkaWdlc3RJRABmcmFuZG9tUOBI6LYJPbIj9cVkXtsP2F5xZWxlbWVudElkZW50aWZpZXJqZ2l2ZW5fbmFtZWxlbGVtZW50VmFsdWVkSm9obtgYWFKkaGRpZ2VzdElEAWZyYW5kb21QDI8xTUeXQEFSjKxLTnTucHFlbGVtZW50SWRlbnRpZmllcmtmYW1pbHlfbmFtZWxlbGVtZW50VmFsdWVjRG9lamlzc3VlckF1dGiEQ6EBJqEYIVkECzCCBAcwggLvoAMCAQICFC91o6_IowD4Ur1EK5mg4nwHU3MHMA0GCSqGSIb3DQEBCwUAMIGSMQswCQYDVQQGEwJERTEPMA0GA1UECAwGSGVzc2VuMRowGAYDVQQHDBFGcmFua2Z1cnQgYW0gTWFpbjEVMBMGA1UECgwMVGVzdCBDb21wYW55MRIwEAYDVQQLDAlUZXN0IFVuaXQxDTALBgNVBAMMBFRlc3QxHDAaBgkqhkiG9w0BCQEWDVRlc3RAdGVzdC5jb20wHhcNMjUwODE0MDY0OTM1WhcNMjYwODE0MDY0OTM1WjCBkjELMAkGA1UEBhMCREUxDzANBgNVBAgMBkhlc3NlbjEaMBgGA1UEBwwRRnJhbmtmdXJ0IGFtIE1haW4xFTATBgNVBAoMDFRlc3QgQ29tcGFueTESMBAGA1UECwwJVGVzdCBVbml0MQ0wCwYDVQQDDARUZXN0MRwwGgYJKoZIhvcNAQkBFg1UZXN0QHRlc3QuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkniO5UBYYy8EFbenFoCw9dzvaRH7Pp4RSLqzV5shg2R1KZJEj03FJ_1ECjcnA6pThrw4V0uwiixLY7bQ5HDV-C9xHxO5GfbScoaRCsrwtm3JucPqQaVc4CgGk---kyp-fomFDmSqOTVqJLrNKMniA5ohIAorS6urB-dKgF8q5AZ_xe_KTCe9qeZ7JDxu_JxMR-7G4P9Ktq0buuyiRhW6hKzLioGzVPGrw79KNYdIUQgdlJmiLzwkYjPrrm-faPLTrzbBRDl_gX2wxS7kVmy_mESFvKMgnX95ae_wKL6Safzbeh3fbgcrPvYXjF2ZyLRSp7AbrYphu_GxOrSR2S8JAwIDAQABo1MwUTAdBgNVHQ4EFgQUUp_qjf-pvURVP-cWUuB9T7TnEkowHwYDVR0jBBgwFoAUUp_qjf-pvURVP-cWUuB9T7TnEkowDwYDVR0TAQH_BAUwAwEB_zANBgkqhkiG9w0BAQsFAAOCAQEAiJyQJrlLJnUBJXWSgQ5oyomSgZFNMUVmDXnL_RT5HByGS4M3Xj-Es3XoFepZQJdYlM1uZx6WWriwioJ3X9RbnznQDnLdC-wU50gIZ1uhiRth_FRRwfgr1aHYzKZg5sL_aEc9B76TRUAUSU8VYWBcladV7SQRqOY5QC9gOptGJUfHp6yKMJ9Di0K6NGdswH_medX0z9v_auOdOCHzvnd_4Kp5YuwdNYRDMeV90GUnqRlvLZAC7PVDbDB2XzIkGY1HmIRuWRRMJES0y2R_EYpGa2viO7PxMmLgPwPaHdOqMav2ibxWchIbfKP1Nr6bGQCxImqN5ZvfBJfPq2vog5fdxlkCDNgYWQIHp2d2ZXJzaW9uYzEuMG9kaWdlc3RBbGdvcml0aG1nU0hBLTI1Nmdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGx2YWx1ZURpZ2VzdHOhcW9yZy5pc28uMTgwMTMuNS4xogBYIDCuG2Pl0G2jwUJm-lpS-Wjt5Vgo_hE70NVYLWM2zhPWAVggpazwAlF4cX7j-NBzDHaflc9yBw5zEyANj-jvqqSWb7BtZGV2aWNlS2V5SW5mb6FpZGV2aWNlS2V5pAECIAEhWCAg-QilggJwXlxnOIFYB0klBZSvnJUAu89zM6p_y6jdkCJYIGZBs-z8vNggFIGbvJ6gjzyMEEype9bNEGLip_9MyRMCbHZhbGlkaXR5SW5mb6Nmc2lnbmVkwHgYMjAyNS0wOC0xM1QxODoyNzo0Mi45MDNaaXZhbGlkRnJvbcB4GDIwMjUtMDgtMTNUMTg6Mjc6NDIuOTAzWmp2YWxpZFVudGlswHgYMjAzMC0wMS0zMFQxNTozODowMS41NjlaZnN0YXR1c6Frc3RhdHVzX2xpc3SiY2lkeBkZIGN1cml4XGh0dHBzOi8vdGVzdC5jb25uZWN0b3IubGlzc2kuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9NzcxOTZhMmMtNWZiMy00MmYzLTliZTAtMTkxNGNmOWQ4NTc4WECH6H12ua6wvdEY4Wb0zz0TA0Q9HOe0esZM7u3liFeZ784p_JCQh8JGSY9JzSB_G3P-VD6uCUF4Gz22FNkMhEpx";
//         var expectedMdoc = "omdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGxpc3N1ZXJTaWduZWSiamlzc3VlckF1dGiEQ6EBJqEYIVkECzCCBAcwggLvoAMCAQICFC91o6_IowD4Ur1EK5mg4nwHU3MHMA0GCSqGSIb3DQEBCwUAMIGSMQswCQYDVQQGEwJERTEPMA0GA1UECAwGSGVzc2VuMRowGAYDVQQHDBFGcmFua2Z1cnQgYW0gTWFpbjEVMBMGA1UECgwMVGVzdCBDb21wYW55MRIwEAYDVQQLDAlUZXN0IFVuaXQxDTALBgNVBAMMBFRlc3QxHDAaBgkqhkiG9w0BCQEWDVRlc3RAdGVzdC5jb20wHhcNMjUwODE0MDY0OTM1WhcNMjYwODE0MDY0OTM1WjCBkjELMAkGA1UEBhMCREUxDzANBgNVBAgMBkhlc3NlbjEaMBgGA1UEBwwRRnJhbmtmdXJ0IGFtIE1haW4xFTATBgNVBAoMDFRlc3QgQ29tcGFueTESMBAGA1UECwwJVGVzdCBVbml0MQ0wCwYDVQQDDARUZXN0MRwwGgYJKoZIhvcNAQkBFg1UZXN0QHRlc3QuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkniO5UBYYy8EFbenFoCw9dzvaRH7Pp4RSLqzV5shg2R1KZJEj03FJ_1ECjcnA6pThrw4V0uwiixLY7bQ5HDV-C9xHxO5GfbScoaRCsrwtm3JucPqQaVc4CgGk---kyp-fomFDmSqOTVqJLrNKMniA5ohIAorS6urB-dKgF8q5AZ_xe_KTCe9qeZ7JDxu_JxMR-7G4P9Ktq0buuyiRhW6hKzLioGzVPGrw79KNYdIUQgdlJmiLzwkYjPrrm-faPLTrzbBRDl_gX2wxS7kVmy_mESFvKMgnX95ae_wKL6Safzbeh3fbgcrPvYXjF2ZyLRSp7AbrYphu_GxOrSR2S8JAwIDAQABo1MwUTAdBgNVHQ4EFgQUUp_qjf-pvURVP-cWUuB9T7TnEkowHwYDVR0jBBgwFoAUUp_qjf-pvURVP-cWUuB9T7TnEkowDwYDVR0TAQH_BAUwAwEB_zANBgkqhkiG9w0BAQsFAAOCAQEAiJyQJrlLJnUBJXWSgQ5oyomSgZFNMUVmDXnL_RT5HByGS4M3Xj-Es3XoFepZQJdYlM1uZx6WWriwioJ3X9RbnznQDnLdC-wU50gIZ1uhiRth_FRRwfgr1aHYzKZg5sL_aEc9B76TRUAUSU8VYWBcladV7SQRqOY5QC9gOptGJUfHp6yKMJ9Di0K6NGdswH_medX0z9v_auOdOCHzvnd_4Kp5YuwdNYRDMeV90GUnqRlvLZAC7PVDbDB2XzIkGY1HmIRuWRRMJES0y2R_EYpGa2viO7PxMmLgPwPaHdOqMav2ibxWchIbfKP1Nr6bGQCxImqN5ZvfBJfPq2vog5fdxlkCDNgYWQIHp2d2ZXJzaW9uYzEuMG9kaWdlc3RBbGdvcml0aG1nU0hBLTI1Nmdkb2NUeXBldW9yZy5pc28uMTgwMTMuNS4xLm1ETGx2YWx1ZURpZ2VzdHOhcW9yZy5pc28uMTgwMTMuNS4xogBYIDCuG2Pl0G2jwUJm-lpS-Wjt5Vgo_hE70NVYLWM2zhPWAVggpazwAlF4cX7j-NBzDHaflc9yBw5zEyANj-jvqqSWb7BtZGV2aWNlS2V5SW5mb6FpZGV2aWNlS2V5pAECIAEhWCAg-QilggJwXlxnOIFYB0klBZSvnJUAu89zM6p_y6jdkCJYIGZBs-z8vNggFIGbvJ6gjzyMEEype9bNEGLip_9MyRMCbHZhbGlkaXR5SW5mb6Nmc2lnbmVkwHgYMjAyNS0wOC0xM1QxODoyNzo0Mi45MDNaaXZhbGlkRnJvbcB4GDIwMjUtMDgtMTNUMTg6Mjc6NDIuOTAzWmp2YWxpZFVudGlswHgYMjAzMC0wMS0zMFQxNTozODowMS41NjlaZnN0YXR1c6Frc3RhdHVzX2xpc3SiY2lkeBkZIGN1cml4XGh0dHBzOi8vdGVzdC5jb25uZWN0b3IubGlzc2kuaW8vc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9NzcxOTZhMmMtNWZiMy00MmYzLTliZTAtMTkxNGNmOWQ4NTc4WECH6H12ua6wvdEY4Wb0zz0TA0Q9HOe0esZM7u3liFeZ784p_JCQh8JGSY9JzSB_G3P-VD6uCUF4Gz22FNkMhEpxam5hbWVTcGFjZXOhcW9yZy5pc28uMTgwMTMuNS4xgtgYWFKkaGRpZ2VzdElEAGZyYW5kb21Q4Ejotgk9siP1xWRe2w_YXnFlbGVtZW50SWRlbnRpZmllcmpnaXZlbl9uYW1lbGVsZW1lbnRWYWx1ZWRKb2hu2BhYUqRoZGlnZXN0SUQBZnJhbmRvbVAMjzFNR5dAQVKMrEtOdO5wcWVsZW1lbnRJZGVudGlmaWVya2ZhbWlseV9uYW1lbGVsZW1lbnRWYWx1ZWNEb2U";
//
//         var mdoc = Mdoc.FromIssuerSigned(encodedMdoc).UnwrapOrThrow();
//         var keyId = KeyId.CreateKeyId();
//         var record = mdoc.ToRecord(Option<List<MdocDisplay>>.None, keyId, CredentialSetId.CreateCredentialSetId(), false);
//
//         var jsonString = MdocRecordSerializer.Serialize(record);
//         var sut = JObject.Parse(jsonString);
//
//         sut[nameof(RecordBase.Id)]!.ToString().Should().Be(record.Id);
//         sut[MdocRecordSerializationConstants.MdocJsonKey]!.ToString().Should().Be(expectedMdoc);
//         sut[MdocRecordSerializationConstants.KeyIdJsonKey]!.ToString().Should().Be(keyId.ToString());
//     }
//
//     [Fact]
//     public void Can_Decode_From_Json()
//     {
//         var json = MdocVcSamples.MdocRecordJson;
//
//         var sut = MdocRecordSerializer.Deserialize(json.ToString());
//
//         sut.Match(
//             record =>
//             {
//                 record.Mdoc.DocType.ToString().Should().Be(MdocSamples.DocType);
//             },
//             errors =>
//             {
//                 Assert.Fail("Should not have errors");
//             }
//         );
//     }
// }
