using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.SdJwtLib.Models
{
    public struct PresentationFormat
    {
        public string Value { get; }

        public PresentationFormat(string value) => Value = value;
    }

    public static class PresentationFormatExtensions
    {
        public static string AddKeyBindingJwt(this PresentationFormat presentationFormat, string kbJwt)
        {
            return presentationFormat.Value + kbJwt;
        }
        
        public static string ToSdHash(this PresentationFormat presentationFormat)
        {
            //TODO: Use _sd_alg to hash the presentation format
            var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(presentationFormat.Value));
            return Base64UrlEncoder.Encode(bytes);
        }
    }
}
