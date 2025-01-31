using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
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
    public async Task<Option<CredentialState>> GetState(StatusListEntry statusListEntry)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(statusListEntry.Uri);

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
                                var decompressedBytes = DecompressBytes(bytes);
                    
                                var statusPerByte = 8 / bitSize;
                                var relevantByteLocation = statusListEntry.Idx / statusPerByte;
                                var relevantByte = decompressedBytes[relevantByteLocation];
        
                                var startPointByteIndex = statusListEntry.Idx % statusPerByte;
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
    
    private static byte[] DecompressBytes(byte[] compressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            throw new ArgumentException("Compressed data cannot be null or empty");

        // Check the zlib header (2 bytes)
        if (compressedData.Length < 6)
            throw new InvalidDataException("Compressed data is too short.");

        var cmf = compressedData[0];
        var flg = compressedData[1];

        // Validate compression method (CM) and compression info (CINFO)
        if ((cmf & 0x0F) != 8 || (cmf >> 4) > 7)
            throw new InvalidDataException("Unsupported zlib compression method or info.");

        // Validate the header checksum
        if ((cmf * 256 + flg) % 31 != 0)
            throw new InvalidDataException("Invalid zlib header checksum.");

        // Remove the zlib header (first 2 bytes) and Adler-32 checksum (last 4 bytes)
        var deflateDataLength = compressedData.Length - 6;
        if (deflateDataLength <= 0)
            throw new InvalidDataException("No deflate-compressed data found.");

        var deflateData = new byte[deflateDataLength];
        Array.Copy(compressedData, 2, deflateData, 0, deflateDataLength);

        // Decompress using DeflateStream
        using (var inputStream = new MemoryStream(deflateData))
        using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            deflateStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}
