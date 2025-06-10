using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.DcApi;

public class DcApiRequestTests
{
    [Fact]
    public void Valid_Json_String_Can_Be_Processed()
    {
        // Arrange
        const string validJson = """
                                 {
                                            "dcql_query":{
                                               "credentials":[
                                                  {
                                                     "claims":[
                                                        {
                                                           "path":[
                                                              "org.iso.18013.5.1",
                                                              "family_name"
                                                           ]
                                                        },
                                                        {
                                                           "path":[
                                                              "org.iso.18013.5.1",
                                                              "given_name"
                                                           ]
                                                        }
                                                     ],
                                                     "format":"mso_mdoc",
                                                     "id":"cred1",
                                                     "meta":{
                                                        "doctype_value":"org.iso.18013.5.1.mDL"
                                                     }
                                                  }
                                               ]
                                            },
                                            "nonce":"cQAgOKI-5dXxyhKJI38QX-d_qGLxXgn_1wSYmzeCDTQ",
                                            "response_mode":"dc_api",
                                            "response_type":"vp_token"
                                         }
                                 """;

        // Act
        var result = DcApiRequest.ValidDcApiRequest(validJson);

        // Assert
        result.Match(
            dcApiRequest =>
            {
                dcApiRequest.Nonce.Should().Be("cQAgOKI-5dXxyhKJI38QX-d_qGLxXgn_1wSYmzeCDTQ");
                dcApiRequest.ResponseMode.Should().Be("dc_api");
                dcApiRequest.ResponseType.Should().Be("vp_token");
                
                dcApiRequest.DcqlQuery.Should().NotBeNull();
                dcApiRequest.DcqlQuery.CredentialQueries.Should().HaveCount(1);
                
                var credentialQuery = dcApiRequest.DcqlQuery.CredentialQueries[0];
                credentialQuery.Id.AsString().Should().Be("cred1");
                credentialQuery.Format.Should().Be("mso_mdoc");
                credentialQuery.Meta!.Doctype.Should().Be("org.iso.18013.5.1.mDL");
                credentialQuery.Claims.Should().HaveCount(2);
                
                var firstClaim = credentialQuery.Claims![0];
                firstClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "family_name");
                
                var secondClaim = credentialQuery.Claims![1];
                secondClaim.Path.GetPathComponents().Select(c => c.ToString()).Should().BeEquivalentTo("org.iso.18013.5.1", "given_name");
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }
} 
