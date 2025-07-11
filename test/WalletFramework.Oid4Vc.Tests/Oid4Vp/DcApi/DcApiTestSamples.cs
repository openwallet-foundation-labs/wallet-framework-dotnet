namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.DcApi;

/// <summary>
///     Contains shared test samples for DC-API related tests to avoid duplication.
/// </summary>
public static class DcApiTestSamples
{
    /// <summary>
    ///     Sample DC-API request JSON for testing.
    /// </summary>
    public const string ValidDcApiRequestJson = """
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

    /// <summary>
    ///     Sample DC-API request batch JSON for testing.
    /// </summary>
    public static readonly string ValidDcApiRequestBatchJson = $$"""
                                                                  {
                                                                     "requests":[
                                                                        {
                                                                           "data":{{ValidDcApiRequestJson}},
                                                                           "protocol":"openid4vp"
                                                                        }
                                                                     ]
                                                                  }
                                                                  """;
} 