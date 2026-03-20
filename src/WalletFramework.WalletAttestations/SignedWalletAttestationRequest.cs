using Microsoft.IdentityModel.JsonWebTokens;
using WalletFramework.WalletAttestations.Issuance;

namespace WalletFramework.WalletAttestations;

public record SignedWalletAttestationRequest(JsonWebToken SignedJwt, WalletAttestationRequest Value);
