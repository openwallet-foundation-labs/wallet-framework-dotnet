using System.Text;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Roles;
using WalletFramework.SdJwtLib.Roles.Implementation;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.SdJwtVc.Tests;

public class SdJwtVcHolderServiceTests
{
    private readonly SdJwtVcHolderService _service;

    public SdJwtVcHolderServiceTests()
    {
        IHolder holder = new Holder();
        var sdJwtSignerMock = new Mock<ISdJwtSigner>().Object;
        _service = new SdJwtVcHolderService(holder, sdJwtSignerMock);
    }

    private const string Example4ASdJwt =
        "eyJhbGciOiAiRVMyNTYiLCAidHlwIjogInZjK3NkLWp3dCJ9.eyJfc2QiOiBbIjBIWm1uU0lQejMzN2tTV2U3QzM0bC0tODhnekppLWVCSjJWel9ISndBVGciLCAiOVpicGxDN1RkRVc3cWFsNkJCWmxNdHFKZG1lRU9pWGV2ZEpsb1hWSmRSUSIsICJJMDBmY0ZVb0RYQ3VjcDV5eTJ1anFQc3NEVkdhV05pVWxpTnpfYXdEMGdjIiwgIklFQllTSkdOaFhJbHJRbzU4eWtYbTJaeDN5bGw5WmxUdFRvUG8xN1FRaVkiLCAiTGFpNklVNmQ3R1FhZ1hSN0F2R1RyblhnU2xkM3o4RUlnX2Z2M2ZPWjFXZyIsICJodkRYaHdtR2NKUXNCQ0EyT3RqdUxBY3dBTXBEc2FVMG5rb3ZjS09xV05FIiwgImlrdXVyOFE0azhxM1ZjeUE3ZEMtbU5qWkJrUmVEVFUtQ0c0bmlURTdPVFUiLCAicXZ6TkxqMnZoOW80U0VYT2ZNaVlEdXZUeWtkc1dDTmcwd1RkbHIwQUVJTSIsICJ3elcxNWJoQ2t2a3N4VnZ1SjhSRjN4aThpNjRsbjFqb183NkJDMm9hMXVnIiwgInpPZUJYaHh2SVM0WnptUWNMbHhLdUVBT0dHQnlqT3FhMXoySW9WeF9ZRFEiXSwgImlzcyI6ICJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsICJpYXQiOiAxNjgzMDAwMDAwLCAiZXhwIjogMTg4MzAwMDAwMCwgInZjdCI6ICJodHRwczovL2JtaS5idW5kLmV4YW1wbGUvY3JlZGVudGlhbC9waWQvMS4wIiwgImFnZV9lcXVhbF9vcl9vdmVyIjogeyJfc2QiOiBbIkZjOElfMDdMT2NnUHdyREpLUXlJR085N3dWc09wbE1Makh2UkM0UjQtV2ciLCAiWEx0TGphZFVXYzl6Tl85aE1KUm9xeTQ2VXNDS2IxSXNoWnV1cVVGS1NDQSIsICJhb0NDenNDN3A0cWhaSUFoX2lkUkNTQ2E2NDF1eWNuYzh6UGZOV3o4bngwIiwgImYxLVAwQTJkS1dhdnYxdUZuTVgyQTctRVh4dmhveHY1YUhodUVJTi1XNjQiLCAiazVoeTJyMDE4dnJzSmpvLVZqZDZnNnl0N0Fhb25Lb25uaXVKOXplbDNqbyIsICJxcDdaX0t5MVlpcDBzWWdETzN6VnVnMk1GdVBOakh4a3NCRG5KWjRhSS1jIl19LCAiX3NkX2FsZyI6ICJzaGEtMjU2IiwgImNuZiI6IHsiandrIjogeyJrdHkiOiAiRUMiLCAiY3J2IjogIlAtMjU2IiwgIngiOiAiVENBRVIxOVp2dTNPSEY0ajRXNHZmU1ZvSElQMUlMaWxEbHM3dkNlR2VtYyIsICJ5IjogIlp4amlXV2JaTVFHSFZXS1ZRNGhiU0lpcnNWZnVlY0NFNnQ0alQ5RjJIWlEifX19.jeF9GjGbjCr0NND0SbkV4HeSpsysixALFScJl4bYkIykXhF6cRtqni64_d7X6Ef8Rx80rfsgXe0H7TdiSoIJOw~WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgImdpdmVuX25hbWUiLCAiRXJpa2EiXQ~WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgImZhbWlseV9uYW1lIiwgIk11c3Rlcm1hbm4iXQ~WyI2SWo3dE0tYTVpVlBHYm9TNXRtdlZBIiwgImJpcnRoZGF0ZSIsICIxOTYzLTA4LTEyIl0~WyJlSThaV205UW5LUHBOUGVOZW5IZGhRIiwgInNvdXJjZV9kb2N1bWVudF90eXBlIiwgImlkX2NhcmQiXQ~WyJRZ19PNjR6cUF4ZTQxMmExMDhpcm9BIiwgInN0cmVldF9hZGRyZXNzIiwgIkhlaWRlc3RyYVx1MDBkZmUgMTciXQ~WyJBSngtMDk1VlBycFR0TjRRTU9xUk9BIiwgImxvY2FsaXR5IiwgIktcdTAwZjZsbiJd~WyJQYzMzSk0yTGNoY1VfbEhnZ3ZfdWZRIiwgInBvc3RhbF9jb2RlIiwgIjUxMTQ3Il0~WyJHMDJOU3JRZmpGWFE3SW8wOXN5YWpBIiwgImNvdW50cnkiLCAiREUiXQ~WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgImFkZHJlc3MiLCB7Il9zZCI6IFsiWEZjN3pYUG03enpWZE15d20yRXVCZmxrYTVISHF2ZjhVcF9zek5HcXZpZyIsICJiZDFFVnpnTm9wVWs0RVczX2VRMm4zX05VNGl1WE9IdjlYYkdITjNnMVRFIiwgImZfRlFZZ3ZRV3Z5VnFObklYc0FSbE55ZTdZR3A4RTc3Z1JBamFxLXd2bnciLCAidjRra2JfcFAxamx2VWJTanR5YzVicWNXeUEtaThYTHZoVllZN1pUMHRiMCJdfV0~WyJuUHVvUW5rUkZxM0JJZUFtN0FuWEZBIiwgIm5hdGlvbmFsaXRpZXMiLCBbIkRFIl1d~WyI1YlBzMUlxdVpOYTBoa2FGenp6Wk53IiwgImdlbmRlciIsICJmZW1hbGUiXQ~WyI1YTJXMF9OcmxFWnpmcW1rXzdQcS13IiwgImJpcnRoX2ZhbWlseV9uYW1lIiwgIkdhYmxlciJd~WyJ5MXNWVTV3ZGZKYWhWZGd3UGdTN1JRIiwgImxvY2FsaXR5IiwgIkJlcmxpbiJd~WyJIYlE0WDhzclZXM1FEeG5JSmRxeU9BIiwgInBsYWNlX29mX2JpcnRoIiwgeyJfc2QiOiBbIldwaEhvSUR5b1diQXBEQzR6YnV3UjQweGwweExoRENfY3Y0dHNTNzFyRUEiXSwgImNvdW50cnkiOiAiREUifV0~WyJDOUdTb3VqdmlKcXVFZ1lmb2pDYjFBIiwgImFsc29fa25vd25fYXMiLCAiU2Nod2VzdGVyIEFnbmVzIl0~WyJreDVrRjE3Vi14MEptd1V4OXZndnR3IiwgIjEyIiwgdHJ1ZV0~WyJIM28xdXN3UDc2MEZpMnllR2RWQ0VRIiwgIjE0IiwgdHJ1ZV0~WyJPQktsVFZsdkxnLUFkd3FZR2JQOFpBIiwgIjE2IiwgdHJ1ZV0~WyJNMEpiNTd0NDF1YnJrU3V5ckRUM3hBIiwgIjE4IiwgdHJ1ZV0~WyJEc210S05ncFY0ZEFIcGpyY2Fvc0F3IiwgIjIxIiwgdHJ1ZV0~WyJlSzVvNXBIZmd1cFBwbHRqMXFoQUp3IiwgIjY1IiwgZmFsc2Vd~";
    private const string ComplexStructuredSdJwt =
        "eyJhbGciOiAiRVMyNTYiLCAidHlwIjogImV4YW1wbGUrc2Qtand0In0.eyJfc2QiOiBbIi1hU3puSWQ5bVdNOG9jdVFvbENsbHN4VmdncTEtdkhXNE90bmhVdFZtV3ciLCAiSUticllObjN2QTdXRUZyeXN2YmRCSmpERFVfRXZRSXIwVzE4dlRScFVTZyIsICJvdGt4dVQxNG5CaXd6TkozTVBhT2l0T2w5cFZuWE9hRUhhbF94a3lOZktJIl0sICJpc3MiOiAiaHR0cHM6Ly9pc3N1ZXIuZXhhbXBsZS5jb20iLCAiaWF0IjogMTY4MzAwMDAwMCwgImV4cCI6IDE4ODMwMDAwMDAsICJ2ZXJpZmllZF9jbGFpbXMiOiB7InZlcmlmaWNhdGlvbiI6IHsiX3NkIjogWyI3aDRVRTlxU2N2REtvZFhWQ3VvS2ZLQkpwVkJmWE1GX1RtQUdWYVplM1NjIiwgInZUd2UzcmFISUZZZ0ZBM3hhVUQyYU14Rno1b0RvOGlCdTA1cUtsT2c5THciXSwgInRydXN0X2ZyYW1ld29yayI6ICJkZV9hbWwiLCAiZXZpZGVuY2UiOiBbeyIuLi4iOiAidFlKMFREdWN5WlpDUk1iUk9HNHFSTzV2a1BTRlJ4RmhVRUxjMThDU2wzayJ9XX0sICJjbGFpbXMiOiB7Il9zZCI6IFsiUmlPaUNuNl93NVpIYWFka1FNcmNRSmYwSnRlNVJ3dXJSczU0MjMxRFRsbyIsICJTXzQ5OGJicEt6QjZFYW5mdHNzMHhjN2NPYW9uZVJyM3BLcjdOZFJtc01vIiwgIldOQS1VTks3Rl96aHNBYjlzeVdPNklJUTF1SGxUbU9VOHI4Q3ZKMGNJTWsiLCAiV3hoX3NWM2lSSDliZ3JUQkppLWFZSE5DTHQtdmpoWDFzZC1pZ09mXzlsayIsICJfTy13SmlIM2VuU0I0Uk9IbnRUb1FUOEptTHR6LW1oTzJmMWM4OVhvZXJRIiwgImh2RFhod21HY0pRc0JDQTJPdGp1TEFjd0FNcERzYVUwbmtvdmNLT3FXTkUiXX19LCAiX3NkX2FsZyI6ICJzaGEtMjU2In0.BYUpdUIaUTNj5UlPBk6g0GhDp213yGD_HIMk8P45LwkSAfZgc_ayGnf9VC4gebeE-crDoonxf89Y7qsTA-4qdQ~WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgInRpbWUiLCAiMjAxMi0wNC0yM1QxODoyNVoiXQ~WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgInZlcmlmaWNhdGlvbl9wcm9jZXNzIiwgImYyNGM2Zi02ZDNmLTRlYzUtOTczZS1iMGQ4NTA2ZjNiYzciXQ~WyJlSThaV205UW5LUHBOUGVOZW5IZGhRIiwgIm1ldGhvZCIsICJwaXBwIl0~WyJRZ19PNjR6cUF4ZTQxMmExMDhpcm9BIiwgInRpbWUiLCAiMjAxMi0wNC0yMlQxMTozMFoiXQ~WyJBSngtMDk1VlBycFR0TjRRTU9xUk9BIiwgImRvY3VtZW50IiwgeyJ0eXBlIjogImlkY2FyZCIsICJpc3N1ZXIiOiB7Im5hbWUiOiAiU3RhZHQgQXVnc2J1cmciLCAiY291bnRyeSI6ICJERSJ9LCAibnVtYmVyIjogIjUzNTU0NTU0IiwgImRhdGVfb2ZfaXNzdWFuY2UiOiAiMjAxMC0wMy0yMyIsICJkYXRlX29mX2V4cGlyeSI6ICIyMDIwLTAzLTIyIn1d~WyJQYzMzSk0yTGNoY1VfbEhnZ3ZfdWZRIiwgeyJfc2QiOiBbIjl3cGpWUFd1RDdQSzBuc1FETDhCMDZsbWRnVjNMVnliaEh5ZFFwVE55TEkiLCAiRzVFbmhPQU9vVTlYXzZRTU52ekZYanBFQV9SYy1BRXRtMWJHX3djYUtJayIsICJJaHdGcldVQjYzUmNacTl5dmdaMFhQYzdHb3doM08ya3FYZUJJc3dnMUI0IiwgIldweFE0SFNvRXRjVG1DQ0tPZURzbEJfZW11Y1lMejJvTzhvSE5yMWJFVlEiXX1d~WyJHMDJOU3JRZmpGWFE3SW8wOXN5YWpBIiwgImdpdmVuX25hbWUiLCAiTWF4Il0~WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgImZhbWlseV9uYW1lIiwgIk1cdTAwZmNsbGVyIl0~WyJuUHVvUW5rUkZxM0JJZUFtN0FuWEZBIiwgIm5hdGlvbmFsaXRpZXMiLCBbIkRFIl1d~WyI1YlBzMUlxdVpOYTBoa2FGenp6Wk53IiwgImJpcnRoZGF0ZSIsICIxOTU2LTAxLTI4Il0~WyI1YTJXMF9OcmxFWnpmcW1rXzdQcS13IiwgInBsYWNlX29mX2JpcnRoIiwgeyJjb3VudHJ5IjogIklTIiwgImxvY2FsaXR5IjogIlx1MDBkZXlra3ZhYlx1MDBlNmphcmtsYXVzdHVyIn1d~WyJ5MXNWVTV3ZGZKYWhWZGd3UGdTN1JRIiwgImFkZHJlc3MiLCB7ImxvY2FsaXR5IjogIk1heHN0YWR0IiwgInBvc3RhbF9jb2RlIjogIjEyMzQ0IiwgImNvdW50cnkiOiAiREUiLCAic3RyZWV0X2FkZHJlc3MiOiAiV2VpZGVuc3RyYVx1MDBkZmUgMjIifV0~WyJIYlE0WDhzclZXM1FEeG5JSmRxeU9BIiwgImJpcnRoX21pZGRsZV9uYW1lIiwgIlRpbW90aGV1cyJd~WyJDOUdTb3VqdmlKcXVFZ1lmb2pDYjFBIiwgInNhbHV0YXRpb24iLCAiRHIuIl0~WyJreDVrRjE3Vi14MEptd1V4OXZndnR3IiwgIm1zaXNkbiIsICI0OTEyMzQ1Njc4OSJd~WyI2SWo3dE0tYTVpVlBHYm9TNXRtdlZBIiwgInR5cGUiLCAiZG9jdW1lbnQiXQ~";

    private static SdJwtCredential CreateCredential(string serialized) =>
        new(
            new SdJwtDoc(serialized),
            CredentialId.CreateCredentialId(),
            CredentialSetId.CreateCredentialSetId(),
            KeyId.CreateKeyId(),
            CredentialState.Active,
            false,
            Option<DateTime>.None);

    private static string WithVct(string serialized, string vct)
    {
        var parts = serialized.TrimEnd('~').Split('~');
        var jwtParts = parts[0].Split('.');
        var payloadJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(jwtParts[1]));
        var payload = JObject.Parse(payloadJson);
        payload["vct"] = vct;
        var reEncoded = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payload.ToString(Formatting.None)));
        var newJwt = $"{jwtParts[0]}.{reEncoded}.{jwtParts[2]}";
        var disclosures = string.Join("~", parts.Skip(1));
        return $"{newJwt}~{disclosures}~";
    }

    private static ClaimPath Path(params object[] components) =>
        ClaimPath.FromJArray(new JArray(components)).UnwrapOrThrow();

    [Fact]
    public async Task Can_Create_Presentation_For_Example_4A()
    {
        var sdJwtCredential = CreateCredential(Example4ASdJwt);
        var claimsToDisclose = new[]
        {
            Path("given_name"),
            Path("address", "street_address"),
            Path("nationalities", 0),
        };

        var result = await _service.CreatePresentation(
            sdJwtCredential,
            claimsToDisclose,
            Option<IEnumerable<string>>.None,
            Option<string>.None);

        var doc = new SdJwtDoc(result);
        Assert.NotNull(result);
        doc.UnsecuredPayload.SelectToken("given_name", true);
        doc.UnsecuredPayload.SelectToken("address.street_address", true);
        doc.UnsecuredPayload.SelectToken("nationalities[0]", true);

        Assert.Throws<JsonException>(() => doc.UnsecuredPayload.SelectToken("address.locality", true));
        Assert.Throws<JsonException>(() => doc.UnsecuredPayload.SelectToken("gender", true));
    }

    [Fact]
    public async Task Presents_Array_Element_Via_Integer_Index()
    {
        var sdJwtCredential = CreateCredential(WithVct(ComplexStructuredSdJwt, "example-id-document"));
        var claimsToDisclose = new[]
        {
            Path("verified_claims", "verification", "evidence", 0, "type"),
        };

        var result = await _service.CreatePresentation(
            sdJwtCredential,
            claimsToDisclose,
            Option<IEnumerable<string>>.None,
            Option<string>.None);

        var doc = new SdJwtDoc(result);
        var evidenceZeroType = doc.UnsecuredPayload.SelectToken("verified_claims.verification.evidence[0].type", true);
        Assert.Equal("document", evidenceZeroType!.Value<string>());

        Assert.Throws<JsonException>(() =>
            doc.UnsecuredPayload.SelectToken("verified_claims.verification.evidence[0].method", true));
        Assert.Throws<JsonException>(() =>
            doc.UnsecuredPayload.SelectToken("verified_claims.verification.evidence[0].document", true));
    }

    [Fact]
    public async Task Presents_All_Array_Elements_Via_Null()
    {
        var sdJwtCredential = CreateCredential(WithVct(ComplexStructuredSdJwt, "example-id-document"));
        var claimsToDisclose = new[]
        {
            Path("verified_claims", "verification", "evidence", null!, "type"),
        };

        var result = await _service.CreatePresentation(
            sdJwtCredential,
            claimsToDisclose,
            Option<IEnumerable<string>>.None,
            Option<string>.None);

        var doc = new SdJwtDoc(result);
        var evidenceZeroType = doc.UnsecuredPayload.SelectToken("verified_claims.verification.evidence[0].type", true);
        Assert.Equal("document", evidenceZeroType!.Value<string>());

        Assert.Throws<JsonException>(() =>
            doc.UnsecuredPayload.SelectToken("verified_claims.verification.evidence[0].method", true));
    }
}
