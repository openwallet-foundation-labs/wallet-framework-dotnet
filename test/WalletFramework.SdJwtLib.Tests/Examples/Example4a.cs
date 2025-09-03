namespace WalletFramework.SdJwtLib.Tests.Examples;

/*
 * This example is taken from the SD-JWT spec version 8
 * https://datatracker.ietf.org/doc/html/draft-ietf-oauth-selective-disclosure-jwt-08#name-example-4a-sd-jwt-based-ver
 */
internal class Example4A : BaseExample
{
    public override int NumberOfDisclosures => 21;
  
    public override string UnsecuredPayload => """
                                           {
                                             "iss": "https://issuer.example.com",
                                             "iat": 1683000000,
                                             "exp": 1883000000,
                                             "vct": "https://bmi.bund.example/credential/pid/1.0",
                                             "age_equal_or_over": {
                                               "12": true,
                                               "65": false,
                                               "14": true,
                                               "16": true,
                                               "18": true,
                                               "21": true,
                                               
                                             },
                                             "cnf": {
                                               "jwk": {
                                                 "kty": "EC",
                                                 "crv": "P-256",
                                                 "x": "TCAER19Zvu3OHF4j4W4vfSVoHIP1ILilDls7vCeGemc",
                                                 "y": "ZxjiWWbZMQGHVWKVQ4hbSIirsVfuecCE6t4jT9F2HZQ"
                                               }
                                             },
                                             "given_name": "Erika",
                                             "also_known_as": "Schwester Agnes",
                                             "family_name": "Mustermann",
                                             "gender": "female",
                                             "birthdate": "1963-08-12",
                                             "nationalities": [
                                               "DE"
                                             ],
                                             "birth_family_name": "Gabler",
                                             "source_document_type": "id_card",
                                             "place_of_birth": {
                                               "country": "DE",
                                               "locality": "Berlin"
                                             },
                                             "address": {
                                               "postal_code": "51147",
                                               "street_address": "Heidestraße 17",
                                               "locality": "Köln",
                                               "country": "DE"
                                             }
                                           }
                                           """;

    public override string IssuedSdJwt => "eyJhbGciOiAiRVMyNTYiLCAidHlwIjogInZjK3NkLWp3dCJ9.eyJfc2QiOiBbIjBIWm1uU0lQejMzN2tTV2U3QzM0bC0tODhnekppLWVCSjJWel9ISndBVGciLCAiOVpicGxDN1RkRVc3cWFsNkJCWmxNdHFKZG1lRU9pWGV2ZEpsb1hWSmRSUSIsICJJMDBmY0ZVb0RYQ3VjcDV5eTJ1anFQc3NEVkdhV05pVWxpTnpfYXdEMGdjIiwgIklFQllTSkdOaFhJbHJRbzU4eWtYbTJaeDN5bGw5WmxUdFRvUG8xN1FRaVkiLCAiTGFpNklVNmQ3R1FhZ1hSN0F2R1RyblhnU2xkM3o4RUlnX2Z2M2ZPWjFXZyIsICJodkRYaHdtR2NKUXNCQ0EyT3RqdUxBY3dBTXBEc2FVMG5rb3ZjS09xV05FIiwgImlrdXVyOFE0azhxM1ZjeUE3ZEMtbU5qWkJrUmVEVFUtQ0c0bmlURTdPVFUiLCAicXZ6TkxqMnZoOW80U0VYT2ZNaVlEdXZUeWtkc1dDTmcwd1RkbHIwQUVJTSIsICJ3elcxNWJoQ2t2a3N4VnZ1SjhSRjN4aThpNjRsbjFqb183NkJDMm9hMXVnIiwgInpPZUJYaHh2SVM0WnptUWNMbHhLdUVBT0dHQnlqT3FhMXoySW9WeF9ZRFEiXSwgImlzcyI6ICJodHRwczovL2lzc3Vlci5leGFtcGxlLmNvbSIsICJpYXQiOiAxNjgzMDAwMDAwLCAiZXhwIjogMTg4MzAwMDAwMCwgInZjdCI6ICJodHRwczovL2JtaS5idW5kLmV4YW1wbGUvY3JlZGVudGlhbC9waWQvMS4wIiwgImFnZV9lcXVhbF9vcl9vdmVyIjogeyJfc2QiOiBbIkZjOElfMDdMT2NnUHdyREpLUXlJR085N3dWc09wbE1Makh2UkM0UjQtV2ciLCAiWEx0TGphZFVXYzl6Tl85aE1KUm9xeTQ2VXNDS2IxSXNoWnV1cVVGS1NDQSIsICJhb0NDenNDN3A0cWhaSUFoX2lkUkNTQ2E2NDF1eWNuYzh6UGZOV3o4bngwIiwgImYxLVAwQTJkS1dhdnYxdUZuTVgyQTctRVh4dmhveHY1YUhodUVJTi1XNjQiLCAiazVoeTJyMDE4dnJzSmpvLVZqZDZnNnl0N0Fhb25Lb25uaXVKOXplbDNqbyIsICJxcDdaX0t5MVlpcDBzWWdETzN6VnVnMk1GdVBOakh4a3NCRG5KWjRhSS1jIl19LCAiX3NkX2FsZyI6ICJzaGEtMjU2IiwgImNuZiI6IHsiandrIjogeyJrdHkiOiAiRUMiLCAiY3J2IjogIlAtMjU2IiwgIngiOiAiVENBRVIxOVp2dTNPSEY0ajRXNHZmU1ZvSElQMUlMaWxEbHM3dkNlR2VtYyIsICJ5IjogIlp4amlXV2JaTVFHSFZXS1ZRNGhiU0lpcnNWZnVlY0NFNnQ0alQ5RjJIWlEifX19.jeF9GjGbjCr0NND0SbkV4HeSpsysixALFScJl4bYkIykXhF6cRtqni64_d7X6Ef8Rx80rfsgXe0H7TdiSoIJOw~WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgImdpdmVuX25hbWUiLCAiRXJpa2EiXQ~WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgImZhbWlseV9uYW1lIiwgIk11c3Rlcm1hbm4iXQ~WyI2SWo3dE0tYTVpVlBHYm9TNXRtdlZBIiwgImJpcnRoZGF0ZSIsICIxOTYzLTA4LTEyIl0~WyJlSThaV205UW5LUHBOUGVOZW5IZGhRIiwgInNvdXJjZV9kb2N1bWVudF90eXBlIiwgImlkX2NhcmQiXQ~WyJRZ19PNjR6cUF4ZTQxMmExMDhpcm9BIiwgInN0cmVldF9hZGRyZXNzIiwgIkhlaWRlc3RyYVx1MDBkZmUgMTciXQ~WyJBSngtMDk1VlBycFR0TjRRTU9xUk9BIiwgImxvY2FsaXR5IiwgIktcdTAwZjZsbiJd~WyJQYzMzSk0yTGNoY1VfbEhnZ3ZfdWZRIiwgInBvc3RhbF9jb2RlIiwgIjUxMTQ3Il0~WyJHMDJOU3JRZmpGWFE3SW8wOXN5YWpBIiwgImNvdW50cnkiLCAiREUiXQ~WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgImFkZHJlc3MiLCB7Il9zZCI6IFsiWEZjN3pYUG03enpWZE15d20yRXVCZmxrYTVISHF2ZjhVcF9zek5HcXZpZyIsICJiZDFFVnpnTm9wVWs0RVczX2VRMm4zX05VNGl1WE9IdjlYYkdITjNnMVRFIiwgImZfRlFZZ3ZRV3Z5VnFObklYc0FSbE55ZTdZR3A4RTc3Z1JBamFxLXd2bnciLCAidjRra2JfcFAxamx2VWJTanR5YzVicWNXeUEtaThYTHZoVllZN1pUMHRiMCJdfV0~WyJuUHVvUW5rUkZxM0JJZUFtN0FuWEZBIiwgIm5hdGlvbmFsaXRpZXMiLCBbIkRFIl1d~WyI1YlBzMUlxdVpOYTBoa2FGenp6Wk53IiwgImdlbmRlciIsICJmZW1hbGUiXQ~WyI1YTJXMF9OcmxFWnpmcW1rXzdQcS13IiwgImJpcnRoX2ZhbWlseV9uYW1lIiwgIkdhYmxlciJd~WyJ5MXNWVTV3ZGZKYWhWZGd3UGdTN1JRIiwgImxvY2FsaXR5IiwgIkJlcmxpbiJd~WyJIYlE0WDhzclZXM1FEeG5JSmRxeU9BIiwgInBsYWNlX29mX2JpcnRoIiwgeyJfc2QiOiBbIldwaEhvSUR5b1diQXBEQzR6YnV3UjQweGwweExoRENfY3Y0dHNTNzFyRUEiXSwgImNvdW50cnkiOiAiREUifV0~WyJDOUdTb3VqdmlKcXVFZ1lmb2pDYjFBIiwgImFsc29fa25vd25fYXMiLCAiU2Nod2VzdGVyIEFnbmVzIl0~WyJreDVrRjE3Vi14MEptd1V4OXZndnR3IiwgIjEyIiwgdHJ1ZV0~WyJIM28xdXN3UDc2MEZpMnllR2RWQ0VRIiwgIjE0IiwgdHJ1ZV0~WyJPQktsVFZsdkxnLUFkd3FZR2JQOFpBIiwgIjE2IiwgdHJ1ZV0~WyJNMEpiNTd0NDF1YnJrU3V5ckRUM3hBIiwgIjE4IiwgdHJ1ZV0~WyJEc210S05ncFY0ZEFIcGpyY2Fvc0F3IiwgIjIxIiwgdHJ1ZV0~WyJlSzVvNXBIZmd1cFBwbHRqMXFoQUp3IiwgIjY1IiwgZmFsc2Vd~";

    public override string SecuredPayload => """
                                         {
                                           "_sd": [
                                             "0HZmnSIPz337kSWe7C34l--88gzJi-eBJ2Vz_HJwATg",
                                             "9ZbplC7TdEW7qal6BBZlMtqJdmeEOiXevdJloXVJdRQ",
                                             "I00fcFUoDXCucp5yy2ujqPssDVGaWNiUliNz_awD0gc",
                                             "IEBYSJGNhXIlrQo58ykXm2Zx3yll9ZlTtToPo17QQiY",
                                             "Lai6IU6d7GQagXR7AvGTrnXgSld3z8EIg_fv3fOZ1Wg",
                                             "hvDXhwmGcJQsBCA2OtjuLAcwAMpDsaU0nkovcKOqWNE",
                                             "ikuur8Q4k8q3VcyA7dC-mNjZBkReDTU-CG4niTE7OTU",
                                             "qvzNLj2vh9o4SEXOfMiYDuvTykdsWCNg0wTdlr0AEIM",
                                             "wzW15bhCkvksxVvuJ8RF3xi8i64ln1jo_76BC2oa1ug",
                                             "zOeBXhxvIS4ZzmQcLlxKuEAOGGByjOqa1z2IoVx_YDQ"
                                           ],
                                           "iss": "https://issuer.example.com",
                                           "iat": 1683000000,
                                           "exp": 1883000000,
                                           "vct": "https://bmi.bund.example/credential/pid/1.0",
                                           "age_equal_or_over": {
                                             "_sd": [
                                               "Fc8I_07LOcgPwrDJKQyIGO97wVsOplMLjHvRC4R4-Wg",
                                               "XLtLjadUWc9zN_9hMJRoqy46UsCKb1IshZuuqUFKSCA",
                                               "aoCCzsC7p4qhZIAh_idRCSCa641uycnc8zPfNWz8nx0",
                                               "f1-P0A2dKWavv1uFnMX2A7-EXxvhoxv5aHhuEIN-W64",
                                               "k5hy2r018vrsJjo-Vjd6g6yt7AaonKonniuJ9zel3jo",
                                               "qp7Z_Ky1Yip0sYgDO3zVug2MFuPNjHxksBDnJZ4aI-c"
                                             ]
                                           },
                                           "_sd_alg": "sha-256",
                                           "cnf": {
                                             "jwk": {
                                               "kty": "EC",
                                               "crv": "P-256",
                                               "x": "TCAER19Zvu3OHF4j4W4vfSVoHIP1ILilDls7vCeGemc",
                                               "y": "ZxjiWWbZMQGHVWKVQ4hbSIirsVfuecCE6t4jT9F2HZQ"
                                             }
                                           }
                                         }
                                         """;
}
