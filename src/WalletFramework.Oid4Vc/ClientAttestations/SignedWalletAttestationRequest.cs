using Microsoft.IdentityModel.JsonWebTokens;
using WalletFramework.Oid4Vc.WalletAttestations.Issuance;

namespace WalletFramework.Oid4Vc.ClientAttestations;

public record SignedWalletAttestationRequest(JsonWebToken SignedJwt, WalletAttestationRequest Value);
