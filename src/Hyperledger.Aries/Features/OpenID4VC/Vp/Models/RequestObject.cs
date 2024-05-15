using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Hyperledger.Aries.Features.OpenID4VC.Vp.Extensions;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using static Hyperledger.Aries.Features.OpenId4Vc.Vp.Models.AuthorizationRequest;
using static Newtonsoft.Json.Linq.JArray;
using static System.Convert;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vp.Models
{
    /// <summary>
    ///     Represents a request object as defined in the context of OpenID4VP.
    /// </summary>
    public readonly struct RequestObject
    {
        private JwtSecurityToken Value { get; }

        /// <summary>
        ///     The client ID scheme used to obtain and validate metadata of the verifier.
        /// </summary>
        public ClientIdScheme ClientIdScheme => AuthorizationRequest.ClientIdScheme;

        /// <summary>
        ///     The client ID of the verifier.
        /// </summary>
        public string ClientId => AuthorizationRequest.ClientId;

        private AuthorizationRequest AuthorizationRequest { get; }

        /// <summary>
        ///     Creates a new instance of the <see cref="RequestObject" /> class.
        /// </summary>
        public static implicit operator RequestObject(JwtSecurityToken token) => new(token);

        /// <summary>
        ///     Creates a new instance of the <see cref="RequestObject" /> class.
        /// </summary>
        public static implicit operator JwtSecurityToken(RequestObject requestObject) => requestObject.Value;

        private RequestObject(JwtSecurityToken token)
        {
            AuthorizationRequest = CreateAuthorizationRequest(token.Payload.SerializeToJson());
            Value = token;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="RequestObject" /> class.
        /// </summary>
        public static RequestObject CreateRequestObject(string requestObjectJson)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jwt = tokenHandler.ReadJwtToken(requestObjectJson);

            return new RequestObject(jwt);
        }

        /// <summary>
        ///     Gets the authorization request from the request object.
        /// </summary>
        public AuthorizationRequest ToAuthorizationRequest() => AuthorizationRequest;
    }

    /// <summary>
    ///     Extension methods for <see cref="RequestObject" />.
    /// </summary>
    public static class RequestObjectExtensions
    {
        /// <summary>
        ///     Validates the JWT signature.
        /// </summary>
        /// <returns>The validated request object.</returns>
        /// <exception cref="InvalidOperationException">Throws when validation fails</exception>
        public static RequestObject ValidateJwt(this RequestObject requestObject) 
            => ((JwtSecurityToken)requestObject).IsSignatureValid(requestObject.GetLeafCertificate().GetPublicKey()) 
                ? requestObject 
                : throw new InvalidOperationException("Invalid JWT Signature");
        
        /// <summary>
        ///     Validates the SAN name of the leaf certificate
        /// </summary>
        /// <returns>The validated request object</returns>
        /// <exception cref="InvalidOperationException">Throws when validation fails</exception>
        public static RequestObject ValidateSanName(this RequestObject requestObject)
        {
            var x509Certificate = new X509Certificate2(
                requestObject.GetLeafCertificate().GetEncoded()
            );
            
            return GetSanDnsNames(x509Certificate).Any(sanDnsName => sanDnsName == requestObject.ClientId)
                ? requestObject
                : throw new InvalidOperationException("SAN does not match Client ID");
        }
            
        private static IEnumerable<string> GetSanDnsNames(X509Certificate2 certificate)
        {
            const string sanOid = "2.5.29.17";
            var sanNames = new List<string>();

            foreach (var extension in certificate.Extensions)
            {
                if (extension.Oid.Value != sanOid) 
                    continue;
                
                var sanExtension = (AsnEncodedData)extension;
                var sanData = sanExtension.Format(true);

                foreach (var line in sanData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sanNames.Add(line.Split(':', '=').Last().Trim());
                }
            }

            return sanNames;
        }
                

        /// <summary>
        ///     Validates the trust chain of the leaf certificate
        /// </summary>
        /// <returns>The validated request object</returns>
        /// <exception cref="InvalidOperationException">Throws when validation fails</exception>
        public static RequestObject ValidateTrustChain(this RequestObject requestObject) =>
            requestObject
                .GetCertificates()
                .IsTrustChainValid()
                ? requestObject
                : throw new InvalidOperationException("Validation of trust chain failed");

        internal static List<X509Certificate> GetCertificates(this RequestObject requestObject) =>
            Parse(((JwtSecurityToken)requestObject).Header.X5c)
                .Select(
                    certAsJToken =>
                        new X509CertificateParser()
                            .ReadCertificate(
                                FromBase64String(certAsJToken.ToString())
                            )
                )
                .ToList();

        internal static X509Certificate GetLeafCertificate(this RequestObject requestObject) =>
            GetCertificates(requestObject).First();
    }
}
