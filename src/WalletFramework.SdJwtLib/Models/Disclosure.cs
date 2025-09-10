using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace WalletFramework.SdJwtLib.Models;

public class Disclosure
{
    public string Salt;
    
    public string? Name;
    
    public object Value;

    public string? Path { get; internal set; }
    
    public string Base64UrlEncoded => _base64UrlEncoded ??= Serialize();

    private string? _base64UrlEncoded;

    public Disclosure(string? name, object value)
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Create().GetBytes(bytes);
        Salt = Base64UrlEncoder.Encode(bytes);
        Name = name;
        Value = value;
    }

    private Disclosure(string base64UrlEncoded)
    {
        var decodedInput = Base64UrlEncoder.Decode(base64UrlEncoded);
        
        var array = JArray.Parse(decodedInput) ?? throw new SerializationException($"Could not deserialize given disclosure {base64UrlEncoded}");

        var name = array.Count == 3 
            ? array[1].Value<string>() ?? throw new SerializationException("Name could not be deserialized") 
            : null;
        
        var value = array.Count == 3
            ? array[2]
            : array[1];

        Name = name != "_sd" ? name : throw new SerializationException("Name of disclosure must not be _sd");
        Value = value;
        Salt = array[0].Value<string>() ?? throw new SerializationException("Salt could not be deserialized");
        _base64UrlEncoded = base64UrlEncoded;
    }
    
    
    public static Disclosure Deserialize(string input)
    {
        return new Disclosure(input);
    }
    
    public string Serialize()
    {
        if (_base64UrlEncoded != null) return _base64UrlEncoded;
        
        var array = new[] { Salt, Name, Value };
        var json = JsonConvert.SerializeObject(array);
        return Base64UrlEncoder.Encode(json);
    }

    /// <summary>
    /// Get the hash of the disclosure
    /// </summary>
    /// <returns>The base64url encoded hash of the base64url encoded disclosure json object</returns>
    public string GetDigest(SdAlg hashAlgorithm = SdAlg.SHA256)
    {
        if(hashAlgorithm != SdAlg.SHA256) 
            throw new InvalidOperationException("Unsupported hash algorithm");
        
        var hashValue = _base64UrlEncoded != null ? ComputeDigest(_base64UrlEncoded) : ComputeDigest(Serialize());
        return Base64UrlEncoder.Encode(hashValue);
    }

    private byte[] ComputeDigest(string input, SdAlg hashAlgorithm = SdAlg.SHA256)
    {
        switch (hashAlgorithm)
        {
            case SdAlg.SHA256:
            {
                using var sha26 = SHA256.Create();
                return sha26.ComputeHash(Encoding.ASCII.GetBytes(input));
            }
            default:
                throw new InvalidOperationException("Unsupported hash algorithm");
        }
    }
}
