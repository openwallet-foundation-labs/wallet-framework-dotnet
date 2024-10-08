using System.Security.Cryptography.X509Certificates;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Security.Cose.CoseLabel;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.MdocLib.Constants;

namespace WalletFramework.MdocLib.Security.Cose;

public readonly struct UnprotectedHeaders
{
    public Dictionary<CoseLabel, CBORObject> Value { get; }

    public X509Chain X5Chain { get; }
    
    public CBORObject CertByteString { get; }

    public CBORObject this[CoseLabel key] => Value[key];

    private UnprotectedHeaders(
        Dictionary<CoseLabel, CBORObject> value,
        X509Chain x5Chain,
        CBORObject certByteString)
    {
        Value = value;
        X5Chain = x5Chain;
        CertByteString = certByteString;
    }

    internal static Validation<UnprotectedHeaders> ValidUnprotectedHeaders(CBORObject issuerAuth)
    {
        var toDict = new Func<CBORObject, Validation<Dictionary<CoseLabel, CBORObject>>>(cbor =>
        {
            try
            {
                return cbor.ToDictionary(ValidCoseLabel, Valid);
            }
            catch (Exception e)
            {
                return new CborIsNotAMapOrAnArrayError(cbor.ToString(), e);
            }
        });

        var getChainCbor = new Func<Dictionary<CoseLabel, CBORObject>, CoseLabel, Validation<CBORObject>>(
            (dict, label) =>
            {
                try
                {
                    return dict[label];
                }
                catch (Exception)
                {
                    return new CborFieldNotFoundError(label);
                }
            });

        var decodeChain = new Func<CBORObject, Validation<X509Chain>>(cbor =>
        {
            try
            {
                if (cbor.Type == CBORType.Array)
                {
                    return cbor
                        .Values
                        .Select(byteStringCbor =>
                            from byteString in byteStringCbor.TryGetByteString()
                            select new X509Certificate2(byteString)
                        )
                        .TraverseAll(cert => cert)
                        .OnSuccess(certs =>
                        {
                            var chain = new X509Chain();
                            foreach (var cert in certs)
                            {
                                chain.Build(cert);
                            }

                            return chain;
                        });
                }
                else
                { 
                    return cbor.TryGetByteString().OnSuccess(bytes =>
                    {
                        var cert = new X509Certificate2(bytes);
                        var chain = new X509Chain();
                        chain.Build(cert);
                        return chain;
                    });
                }
            }
            catch (Exception e)
            {
                return new InvalidX509CertificateError(cbor.ToString(), e);
            }
        });

        return
            from headersCbor in issuerAuth.GetByIndex(1)
            from dict in toDict(headersCbor)
            from label in ValidCoseLabel(CBORObject.FromObject(CertificateIndex))
            from chainCbor in getChainCbor(dict, label)
            from chain in decodeChain(chainCbor)
            select new UnprotectedHeaders(dict, chain, chainCbor);
    }

    public record InvalidX509CertificateError(string Value, Exception E)
        : Error($"Could not parse x509 Chain from Header. Header value is {Value}", E);

    public CBORObject Encode()
    {
        var cbor = CBORObject.NewMap();
        cbor[CertificateIndex] = CertByteString;
        return cbor;
    }
}
