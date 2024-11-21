using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace WalletFramework.MdocLib.Security;

public class EcKeyPairGenerator
{
    private AsymmetricCipherKeyPair _keyPair;

    public EcKeyPairGenerator()
    {
        GenerateKeyPair();
    }

    private void GenerateKeyPair()
    {
        var secureRandom = new SecureRandom();
        var keyGenParam = new KeyGenerationParameters(secureRandom, 256);
        
        var keyGen = new ECKeyPairGenerator();
        keyGen.Init(keyGenParam);

        _keyPair = keyGen.GenerateKeyPair();
    }

    public ECPrivateKeyParameters GetPrivateKey()
    {
        return (ECPrivateKeyParameters)_keyPair.Private;
    }

    public ECPublicKeyParameters GetPublicKey()
    {
        return (ECPublicKeyParameters)_keyPair.Public;
    }
}
