using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.SdJwtLib.Roles.Implementation;

public class Verifier : IVerifier
{
    public bool VerifyPresentation(string presentation, string issuerJwk)
    {
        var firstSeparator = presentation.IndexOf("~", StringComparison.Ordinal);
        var finalSeparator = presentation.LastIndexOf("~", StringComparison.Ordinal);
        var jwt = presentation.Substring(0, firstSeparator);
        var holderBinding = presentation.Substring(finalSeparator, presentation.Length - finalSeparator).TrimStart('~');
        var disclosures = presentation
            .Substring(firstSeparator, presentation.Length - jwt.Length - holderBinding.Length)
            .Trim('~')
            .Split('~')
            .ToList();


        var issuerKey = JsonWebKey.Create(issuerJwk);
        VerifyJwt(jwt, issuerKey);
    
        return false;
    }

    private bool VerifyJwt(string jwt, SecurityKey issuerSigningKey)
    {
        var validationParameters = new TokenValidationParameters
        {
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidIssuer = null,
            ValidateIssuerSigningKey = true, // Should be true
            IssuerSigningKeys = new []{issuerSigningKey},
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidateAudience = true
        };
    
        var handler = new JwtSecurityTokenHandler();

        handler.ValidateToken(jwt, validationParameters, out var validatedToken);

        return true;
    }
}
