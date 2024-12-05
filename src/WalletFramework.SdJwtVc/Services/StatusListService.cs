using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models.StatusList;

namespace WalletFramework.SdJwtVc.Services;

public class StatusListService(IHttpClientFactory httpClientFactory) : IStatusListService
{
    public async Task<Option<CredentialState>> GetState(Status status)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(status.Uri);

        if (!response.IsSuccessStatusCode)
            return Option<CredentialState>.None;
        
        var content = await response.Content.ReadAsStringAsync();
        
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(content);
        
        var statusListClaim = jwt.Claims.Find(claim => claim.Type == "status_list");
        return statusListClaim.Match(
            Some: claim =>
            {
                var json = JObject.Parse(claim.Value);
                
                var bits = from bitsJson in json.GetByKey("bits").ToOption()
                    select bitsJson.ToObject<int>();
                    
                var list = from listJson in json.GetByKey("lst").ToOption()
                    select Base64UrlEncoder.DecodeBytes(listJson.ToObject<string>());

                return list.Match(bytes =>
                    {
                        return bits.Match(
                            Some: bitSize =>
                            {
                                var decompressedBytes = DecompressWithZlib(bytes);
                    
                                var statusPerByte = 8 / bitSize;
                                var relevantByteLocation = status.Idx / statusPerByte;
                                var relevantByte = decompressedBytes[relevantByteLocation];
        
                                var startPointByteIndex = status.Idx % statusPerByte;
                                var startPointBitIndex = startPointByteIndex * bitSize;
        
                                var sum = 0;
                                for (int i = startPointBitIndex; i < startPointBitIndex + bitSize; i++)
                                {
                                    var bit = new BitArray(new byte[]{relevantByte}).Get(i);
                                    if (bit)
                                        sum += 1 << (i % bitSize);
                                }

                                return sum switch
                                {
                                    0x00 => CredentialState.Active,
                                    0x01 => CredentialState.Revoked,
                                    _ => Option<CredentialState>.None
                                };
                            },
                            None: () => Option<CredentialState>.None
                            );
                },
                    None: () => Option<CredentialState>.None);
            },
            None: () => Option<CredentialState>.None);
    }
    
    private static byte[] DecompressWithZlib(byte[] compressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            throw new ArgumentException("Compressed data cannot be null or empty");

        using (var inputMemoryStream = new MemoryStream(compressedData))
        using (var outputMemoryStream = new MemoryStream())
        {
            using (var zlibStream = new InflaterInputStream(inputMemoryStream))
                zlibStream.CopyTo(outputMemoryStream);

            return outputMemoryStream.ToArray();
        }
    }
}
