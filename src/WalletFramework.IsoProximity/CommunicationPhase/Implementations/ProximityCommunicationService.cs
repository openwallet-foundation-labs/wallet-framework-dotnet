using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.IsoProximity.CommunicationPhase.Abstractions;
using WalletFramework.IsoProximity.EngagementPhase.Abstractions;
using WalletFramework.MdocLib.Ble.Abstractions;
using WalletFramework.MdocLib.Ble.BleUuids;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocLib.Reader;
using WalletFramework.MdocLib.Security;

namespace WalletFramework.IsoProximity.CommunicationPhase.Implementations;

public class ProximityCommunicationService(
    IAesGcmEncryption aes,
    IBleCentral central,
    IEngagementService engagementService) : IProximityCommunicationService
{
    public async Task<(DeviceRequest, SessionTranscript, ECPrivateKeyParameters)> HandleReaderEngagement(ReaderEngagement readerEngagement)
    {
        aes.ResetMessageCounter();
        
        var serviceUuid = readerEngagement.GetServiceUuid();
        Debug.WriteLine($"ServiceUUID is {serviceUuid.ToString()} at {DateTime.Now:H:mm:ss:fff}");

        await central.Init(serviceUuid);
        
        Debug.WriteLine($"Starting session at {DateTime.Now:H:mm:ss:fff}");
        await StartSession(serviceUuid);

        DeviceRequest? result = null;
        SessionTranscript? sessionTranscript = null;
        
        var generator = new EcKeyPairGenerator();
        var pub = generator.GetPublicKey();
        var priv = generator.GetPrivateKey();
        
        var deviceEngagement = await engagementService.CreateDeviceEngagement(pub.ToPubKey());

        var isTimeout = false;
        
        central
            .WaitFor(serviceUuid, MdocReaderUuids.Server2Client, readerEngagement, priv, deviceEngagement)
            .ToObservable()
            .Timeout(TimeSpan.FromSeconds(10))
            .Catch<(DeviceRequest, SessionTranscript), Exception>(exception =>
            {
                isTimeout = true;
                return Observable.Empty<(DeviceRequest, SessionTranscript)>();
            })
            .Subscribe(x =>
            {
                result = x.Item1;
                sessionTranscript = x.Item2;
            });

        Debug.WriteLine($"Writing device engagement at {DateTime.Now:H:mm:ss:fff}");
        await central.Write(
            serviceUuid,
            MdocReaderUuids.Client2Server,
            deviceEngagement.ToCbor().EncodeToBytes());
        
        while (result == null && !isTimeout)
        {
            await Task.Delay(10);
        }
        
        return (result, sessionTranscript!, priv);
    }

    private async Task StartSession(BleUuid serviceUuid)
    {
        await central.Write(serviceUuid, MdocReaderUuids.State, [0x01]);
    }
}

public static class SessionKeyDerivation
{
    public static byte[] DeriveSessionKey(
        ECPrivateKeyParameters privateKey,
        ECPublicKeyParameters publicKey,
        string info,
        byte[] sessionTranscriptBytes)
    {
        // Step 1: Perform ECDH key agreement to compute ZAB
        var ecdh = new ECDHBasicAgreement();
        ecdh.Init(privateKey);
        var zab = ecdh.CalculateAgreement(publicKey).ToByteArrayUnsigned();

        // Step 2: Prepare HKDF parameters
        var ikm = zab;
        var salt = new Sha256Digest();
        salt.BlockUpdate(sessionTranscriptBytes, 0, sessionTranscriptBytes.Length);
        var saltHash = new byte[salt.GetDigestSize()];
        salt.DoFinal(saltHash, 0);

        var hkdf = new HkdfBytesGenerator(new Sha256Digest());
        hkdf.Init(new HkdfParameters(ikm, saltHash, Encoding.UTF8.GetBytes(info)));

        // Step 3: Generate the session key (L=32 octets)
        var sessionKey = new byte[32];
        hkdf.GenerateBytes(sessionKey, 0, sessionKey.Length);

        return sessionKey;
    } 
}
