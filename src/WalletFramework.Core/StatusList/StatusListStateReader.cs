using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Core.StatusList;

internal static class StatusListStateReader
{
    private const string StatusListClaimType = "status_list";
    private const string BitsJsonKey = "bits";
    private const string ListJsonKey = "lst";

    public static Option<CredentialState> GetState(string token, int idx)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var statusListClaim = jwt.Claims.Find(claim => claim.Type == StatusListClaimType);

        return statusListClaim.Match(
            Some: claim => GetStateFromClaim(claim.Value, idx),
            None: () => Option<CredentialState>.None);
    }

    private static Option<CredentialState> GetStateFromClaim(string claimValue, int idx)
    {
        var json = JObject.Parse(claimValue);

        return from bitSize in ReadBitSize(json)
            from compressedList in ReadList(json)
            let decompressedList = DecompressBytes(compressedList)
            from state in GetStateFromBytes(decompressedList, bitSize, idx)
            select state;
    }

    private static Option<int> ReadBitSize(JObject json)
    {
        var bitSize = from bitsJson in json.GetByKey(BitsJsonKey).ToOption()
            select bitsJson.ToObject<int>();

        return bitSize.Match(
            Some: value => IsValidBitSize(value) ? Option<int>.Some(value) : Option<int>.None,
            None: () => Option<int>.None);
    }

    private static Option<byte[]> ReadList(JObject json) =>
        json.GetByKey(ListJsonKey).ToOption().Match(
            Some: DecodeList,
            None: () => Option<byte[]>.None);

    private static Option<byte[]> DecodeList(JToken listJson)
    {
        var encodedList = listJson.ToObject<string>();
        if (string.IsNullOrWhiteSpace(encodedList))
        {
            return Option<byte[]>.None;
        }

        return Base64UrlEncoder.DecodeBytes(encodedList);
    }

    private static Option<CredentialState> GetStateFromBytes(
        byte[] decompressedBytes,
        int bitSize,
        int idx)
    {
        if (idx < 0)
        {
            return Option<CredentialState>.None;
        }

        var statusPerByte = 8 / bitSize;
        var relevantByteLocation = idx / statusPerByte;
        if (relevantByteLocation < decompressedBytes.Length)
        {
            var relevantByte = decompressedBytes[relevantByteLocation];
            var startPointByteIndex = idx % statusPerByte;
            var startPointBitIndex = startPointByteIndex * bitSize;

            var sum = 0;
            var bits = new BitArray([relevantByte]);
            for (var i = startPointBitIndex; i < startPointBitIndex + bitSize; i++)
            {
                if (bits.Get(i))
                {
                    sum += 1 << (i % bitSize);
                }
            }

            return ToCredentialState(sum);
        }

        return Option<CredentialState>.None;
    }

    private static bool IsValidBitSize(int bitSize) =>
        bitSize is 1 or 2 or 4 or 8;

    private static Option<CredentialState> ToCredentialState(int status) =>
        status switch
        {
            0x00 => Option<CredentialState>.Some(CredentialState.Active),
            0x01 => Option<CredentialState>.Some(CredentialState.Revoked),
            _ => Option<CredentialState>.None
        };

    private static byte[] DecompressBytes(byte[] compressedData)
    {
        if (compressedData.Length == 0)
        {
            throw new ArgumentException("Compressed data cannot be empty");
        }

        if (compressedData.Length < 6)
        {
            throw new InvalidDataException("Compressed data is too short.");
        }

        var cmf = compressedData[0];
        var flg = compressedData[1];

        if ((cmf & 0x0F) != 8 || (cmf >> 4) > 7)
        {
            throw new InvalidDataException("Unsupported zlib compression method or info.");
        }

        if ((cmf * 256 + flg) % 31 != 0)
        {
            throw new InvalidDataException("Invalid zlib header checksum.");
        }

        var deflateDataLength = compressedData.Length - 6;
        if (deflateDataLength <= 0)
        {
            throw new InvalidDataException("No deflate-compressed data found.");
        }

        var deflateData = new byte[deflateDataLength];
        Array.Copy(compressedData, 2, deflateData, 0, deflateDataLength);

        using var inputStream = new MemoryStream(deflateData);
        using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        deflateStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
}
