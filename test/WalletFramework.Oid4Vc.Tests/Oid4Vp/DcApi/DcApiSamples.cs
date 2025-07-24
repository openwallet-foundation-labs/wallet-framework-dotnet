namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.DcApi;

/// <summary>
///     Contains shared test samples for DC-API related tests to avoid duplication.
/// </summary>
public static class DcApiSamples
{
    /// <summary>
    ///     Sample DC-API request JSON for testing.
    /// </summary>
    public const string ValidUnsignedDcApiRequestJson = """
                                                {
                                                   "data":{
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
                                                   },
                                                   "protocol":"openid4vp-v1-unsigned"
                                                }
                                                """;

    /// <summary>
    ///     Sample DC-API signed request JSON for testing.
    /// </summary>
    public const string ValidSignedDcApiRequestJson = """
                                                {
                                                  "protocol": "openid4vp-v1-signed",
                                                  "data": {
                                                    "request": "eyJ4NWMiOlsiTUlJRVZ6Q0NBeitnQXdJQkFnSVVPbmRKVVZtRU9hVC9Hb1MwN1hJbktYL2pxd0l3RFFZSktvWklodmNOQVFFTEJRQXdnWUV4Q3pBSkJnTlZCQVlUQWtSRk1ROHdEUVlEVlFRSURBWklaWE56Wlc0eEVqQVFCZ05WQkFjTUNWZHBaWE5pWVdSbGJqRVlNQllHQTFVRUNnd1BSWGhoYlhCc1pTQkRiMjF3WVc1NU1Rc3dDUVlEVlFRTERBSkpWREVtTUNRR0ExVUVBd3dkWkdWdGJ5NWpaWEowYVdacFkyRjBhVzl1TG05d1pXNXBaQzV1WlhRd0hoY05NalF3T1RBeU1UVXhPREE1V2hjTk1qVXdPVEF5TVRVeE9EQTVXakNCZ1RFTE1Ba0dBMVVFQmhNQ1JFVXhEekFOQmdOVkJBZ01Ca2hsYzNObGJqRVNNQkFHQTFVRUJ3d0pWMmxsYzJKaFpHVnVNUmd3RmdZRFZRUUtEQTlGZUdGdGNHeGxJRU52YlhCaGJua3hDekFKQmdOVkJBc01Ba2xVTVNZd0pBWURWUVFEREIxa1pXMXZMbU5sY25ScFptbGpZWFJwYjI0dWIzQmxibWxrTG01bGREQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCQU56WGk2cWdEOFdPWnkwUEphcjIvK1M1bmp6OG1BaGY5QWRjZ2pwa2NmbS8yZW9ZNUpHa0JGbzk3L0pFcytkVTUwSnNmV0JNUTNFaTZ5WmFJc0Y2bWdzVjg3aDFheG1XbnNUUUVjVCtjd1FJK2VHVC9EQVEwNEExekxDbDdyaTAyZWozYU93YjRXSzh3VktQZG1VOS83OENlMWVmd2cxSStIc0RQVnFWUkNOUHhGcytCc2NrTlFkcE5reHA4N3Y5SU5acUpmWmlzNDlBWVliUmRzeG5mb3pMK3RHU1psMkppNHhQZnAraDlLNndxbW00Y1BPTFJ1Ty92VGFxd3FXaFhBWlh1dkNWT0lCWFZzUnNZclN4aWZMMVg4UUVwQUZ1MktBWGhKL3lvL05hYTdzcTZzSERPcEZCV2tLM0txYkp2dlpjZ3Z6blJEdmFiMlpDbWFhaWNUc0NBd0VBQWFPQnhEQ0J3VEJxQmdOVkhSRUVZekJoZ2gxa1pXMXZMbU5sY25ScFptbGpZWFJwYjI0dWIzQmxibWxrTG01bGRJSVBkM2QzTG1WNFlXMXdiR1V1WTI5dGdoQnRZV2xzTG1WNFlXMXdiR1V1WTI5dGdoRmhibTkwYUdWeVpHOXRZV2x1TG1OdmJZY0V3S2dCQVljRXdLZ0JBakFUQmdOVkhTVUVEREFLQmdnckJnRUZCUWNEQWpBZEJnTlZIUTRFRmdRVW96UUdBZFZZc0c5cDdkMDI2cDhlTHdCMVRyMHdId1lEVlIwakJCZ3dGb0FVUmNDdUVjNXJPYnAyd1RLVUs5SzE3bC9MT3lZd0RRWUpLb1pJaHZjTkFRRUxCUUFEZ2dFQkFIdHVzNFdLNmZpRUF0Ulhma0FFUkoybzZnWXBNdTlicXN4N3QyN1g3NUQ2MXJsZ2c1SXo0bm9nU29lWE9yNzhFcXdJTEFZRXRwWjN6SWZ4d0tuSXJ1dmdxMmpEN1IvbWhWaFFwS2M1WmFmVDgyZVZWTkZxeWhudnhST29HN0pUamFZbWNkZENQS3NZYVdxVEErNHMxR1dQbGNvc3piQlpKYWFEYzRVbEIxbTZvaFRUWFk2N1FCR0t1Szlic24xeE5mcnZ1Q05mK1FsNUsxd3RacGNETXpEenFIU0dWazB0MHQxdnVvdDhET0xMY2JDY29lMFUzbUtZK0VaK1BoczhCTTJ5ZDUxbWE1WFc3Y1JlTkZhaTh5bkluN3lDT1ptdG1BSFNMb1pvMjhLYWh1VDVTVXZGVnJJMjBFMDhDMzByTzJtVGFBYXoxODJaZGlZZndlQnVvTkU9IiwiTUlJRUlUQ0NBd21nQXdJQkFnSVVPTUdrZ0NEeHFWczZuZHZvMTNxNmRzM2ZmczB3RFFZSktvWklodmNOQVFFTEJRQXdnWUV4Q3pBSkJnTlZCQVlUQWtSRk1ROHdEUVlEVlFRSURBWklaWE56Wlc0eEVqQVFCZ05WQkFjTUNWZHBaWE5pWVdSbGJqRVlNQllHQTFVRUNnd1BSWGhoYlhCc1pTQkRiMjF3WVc1NU1Rc3dDUVlEVlFRTERBSkpWREVtTUNRR0ExVUVBd3dkWkdWdGJ5NWpaWEowYVdacFkyRjBhVzl1TG05d1pXNXBaQzV1WlhRd0hoY05NalF3T1RBeU1UVTBOVFUzV2hjTk1qVXdPVEF5TVRVME5UVTNXakNCZ1RFTE1Ba0dBMVVFQmhNQ1JFVXhEekFOQmdOVkJBZ01Ca2hsYzNObGJqRVNNQkFHQTFVRUJ3d0pWMmxsYzJKaFpHVnVNUmd3RmdZRFZRUUtEQTlGZUdGdGNHeGxJRU52YlhCaGJua3hDekFKQmdOVkJBc01Ba2xVTVNZd0pBWURWUVFEREIxa1pXMXZMbU5sY25ScFptbGpZWFJwYjI0dWIzQmxibWxrTG01bGREQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCQUxkclRIdlY5ZkY3KzhTYm8rUTRRVEEzMmhkdFFMTWF6VDdWblJLMU9HNk03RndrYkVjYzZORW0zYzdaQVplbHZCUTR2RGdTdFdxMmRFR25nbERYVkhtMW1ncktEcVp3VnRPZk1YZHlYemdZeTlwc2puRlZvaCs5UGVpZkxWT1N2WVgzT09rN01nWGMzUStKd0hQcGpxaDFIeGZhVW5FUVcrYnk2b21wd2UwYnhsTG5wdFJZa09RZUlmdG9kU0tJNXhDNDk4Q0lJWTlCOXNuSVk5aHdJK295UHE5SjNic3k0dGJ3bmNsU1pvQTlZNXlEM25mcTh3VHBRdG9pZm5KWkRGR1VPOGxmb1VxOXpQOEVZSVFVeXRFQTZ5UHdaSmVQTHdrdGtUdUs5QWtoampOL0VuT1BVSUZ5NTVkMmVEbUNDVjVVenJhQkdMbkU4TCt4MStINDZROENBd0VBQWFPQmpqQ0JpekJxQmdOVkhSRUVZekJoZ2gxa1pXMXZMbU5sY25ScFptbGpZWFJwYjI0dWIzQmxibWxrTG01bGRJSVBkM2QzTG1WNFlXMXdiR1V1WTI5dGdoQnRZV2xzTG1WNFlXMXdiR1V1WTI5dGdoRmhibTkwYUdWeVpHOXRZV2x1TG1OdmJZY0V3S2dCQVljRXdLZ0JBakFkQmdOVkhRNEVGZ1FVUmNDdUVjNXJPYnAyd1RLVUs5SzE3bC9MT3lZd0RRWUpLb1pJaHZjTkFRRUxCUUFEZ2dFQkFFRS9WTzloWlc4alE1Q1NyeDNWTEsyYlA1dnpZcVRaaVhIMUFGZVlhei9mSTdRYlBtWlg1K3U3OVJZbXd0L3lPQW5QZjhUV2VBQmI3dVNWTDV6Sm51YlNjTkZ1QkVobW1UcmNPcWJQcExoRlB6UjcvYmVOZTltOWh5cG03V1ZFRlYzdVBLTUxhd2YybUx5VW95Zy9xSWg2MlgxSVk4enVFN1pzaGZWenB0NW5UWHFVRGc2dUY3TzQ1LzlPTnNEOWszNFBIVUt4cVhScHJVL2hxaUhhM3VqeXdkT3RtVGhhN1dQQ0R2ejZMNGlRUEFKWXFnb3NlejJ6OHAyZnkvd0d3VEdOSkdoY242OVRWWXN1Ylczd2lVL0ZRdnFYdzZ3bzFRNm5ldG5RNFhIMnRKMUJlWXB3ZHpSTGdlNTcwYjN0aDRtQ0N3WEl1R0xpd01zYWVxNDVKUDg9Il0sImtpZCI6IjRjNDAxODA2LWI5ZDMtNGI1OC1iODI2LTAyMTk5ZjFmM2M3ZiIsInR5cCI6Im9hdXRoLWF1dGh6LXJlcStqd3QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiJodHRwczovL3NlbGYtaXNzdWVkLm1lL3YyIiwia2RId0hON3FuNGpqMkYzTSI6IlVBQkthN1h5OFpZdmZhOG4iLCJyZXNwb25zZV90eXBlIjoidnBfdG9rZW4iLCJleHBlY3RlZF9vcmlnaW5zIjpbImh0dHBzOi8vZGVtby5jZXJ0aWZpY2F0aW9uLm9wZW5pZC5uZXQiXSwiZGNxbF9xdWVyeSI6eyJjcmVkZW50aWFscyI6W3siY2xhaW1zIjpbeyJwYXRoIjpbIm9yZy5pc28uMTgwMTMuNS4xIiwiZmFtaWx5X25hbWUiXX0seyJwYXRoIjpbIm9yZy5pc28uMTgwMTMuNS4xIiwiZ2l2ZW5fbmFtZSJdfV0sImZvcm1hdCI6Im1zb19tZG9jIiwiaWQiOiJjcmVkMSIsIm1ldGEiOnsiZG9jdHlwZV92YWx1ZSI6Im9yZy5pc28uMTgwMTMuNS4xLm1ETCJ9fV19LCJub25jZSI6ImRoR2R3M0hRMGRIZk9abExrRUI3OEpJdVBaY24tLl9-IiwiY2xpZW50X2lkIjoieDUwOV9zYW5fZG5zOmRlbW8uY2VydGlmaWNhdGlvbi5vcGVuaWQubmV0IiwiY2xpZW50X21ldGFkYXRhIjp7InZwX2Zvcm1hdHNfc3VwcG9ydGVkIjp7Im1zb19tZG9jIjp7ImFsZyI6WyJFUzI1NiJdfX0sImp3a3MiOnsia2V5cyI6W3sia3R5IjoiRUMiLCJ1c2UiOiJlbmMiLCJjcnYiOiJQLTI1NiIsImtpZCI6IjY2N2RkMTgyLTllOTYtNDE0Mi1hY2JhLTY5MDU1MDZmZjMwNiIsIngiOiJtWG5xVHFPZXRXTmVoc29hTXFKY1EwMU00a2UxdXhjbnUyZElQT0Y4TUZZIiwieSI6IjVzUVRvOGl6N2cyUDJyU1BMUktrRkNuX20tcHJNbTF1TTJVYzdfUlB1Tk0iLCJhbGciOiJFQ0RILUVTIn1dfSwiZW5jcnlwdGVkX3Jlc3BvbnNlX2VuY192YWx1ZXNfc3VwcG9ydGVkIjpbIkEyNTZHQ00iXX0sInJlc3BvbnNlX21vZGUiOiJkY19hcGkuand0In0.Ezb9vVrOI58gfAmP1gCvoz9iZDLv5eiGpS2qY0ph_saaFLbE6vVVUXHvlE1pLvkuurygUNdttK_55MZKsYCP0n841HPqoOYC3REUw9sWzBgbbsPlCQ4nARTaIdTfeywtOd9SCeMcXSjl3_UjKmulZR-KuKSvhoTcCHkV8R44NRPo5fqj16CBmV0UZjog3oZ2kVdUr_fEdK52Wv3sHvp6Of4kw_TSCdFjURs7NHN_zsWmdY2U7uIkNSuy4v8v8KPt9C5b8MIK8ZdsS0sVpWMAb0eqySafCVU0Gcg1VHlPMGW1jGaQycshsMDEFgpaRk3dGQmsmpfAjbQxXGKgrHJ-JQ"
                                                  }
                                                }
                                                """;

    /// <summary>
    ///     Sample DC-API request batch JSON for testing.
    /// </summary>
    public static readonly string ValidDcApiUnsignedRequestBatchJson = $$"""
                                                                  {
                                                                     "requests":[
                                                                        {{ValidUnsignedDcApiRequestJson}}
                                                                     ]
                                                                  }
                                                                  """;

    /// <summary>
    ///     Sample DC-API signed request batch JSON for testing.
    /// </summary>
    public static readonly string ValidDcApiSignedRequestBatchJson = $$"""
                                                                  {
                                                                     "requests":[
                                                                        {{ValidSignedDcApiRequestJson}}
                                                                     ]
                                                                  }
                                                                  """;
} 
