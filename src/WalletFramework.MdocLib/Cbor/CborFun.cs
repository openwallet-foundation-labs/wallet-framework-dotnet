using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor.Errors;

namespace WalletFramework.MdocLib.Cbor;

public static class CborFun
{
    public static bool IsNull(this CBORObject? cborObject) => cborObject is null || cborObject.IsNull;
    
    public static Validation<CBORObject> GetByLabel(this CBORObject cborObject, CBORObject label)
    {
        CBORObject value;
        try
        {
            value = cborObject[label];
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(cborObject.ToString(), label.ToString(), e);
        }
        
        if (value.IsNull())
        {
            return new CborFieldNotFoundError(label.ToString());
        }

        return value;
    }

    public static Validation<CBORObject> GetByLabel(this CBORObject cborObject, string label)
    {
        var cborLabel = CBORObject.FromObject(label);
        return cborObject.GetByLabel(cborLabel);
    }

    public static Validation<CBORObject> GetByLabel(this CBORObject cborObject, int label)
    {
        var cborLabel = CBORObject.FromObject(label);
        return cborObject.GetByLabel(cborLabel);
    }

    public static Validation<CBORObject> GetByIndex(this CBORObject cbor, uint index)
    {
        CBORObject value;
        try
        {
            value = cbor[(int)index];
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(cbor.ToString(), index.ToString(), e);
        }
        
        if (value.IsNull())
        {
            return new IndexOutsideOfCborBoundsError(value.ToString(), index);
        }

        return value;
    }
    
    // TODO: Refactor or check with any
    public static Validation<Dictionary<T1, T2>> ToDictionary<T1, T2>(
        this CBORObject cborMap,
        Func<CBORObject, Validation<T1>> keyValidation,
        Func<CBORObject, Validation<T2>> valueValidation) where T1 : notnull
    {
        try
        {
            return cborMap
                .Entries
                .Select(pair =>
                    from key in keyValidation(pair.Key)
                    from value in valueValidation(pair.Value)
                    select new KeyValuePair<T1, T2>(key, value))
                .TraverseAll(pair => pair)
                .OnSuccess(pairs => pairs.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value
                ));
        }
        catch (Exception e)
        {
            return new CborIsNotAMapOrAnArrayError(cborMap.ToString(), e);
        }
    }

    public static Validation<byte[]> TryGetByteString(this CBORObject cbor)
    {
        try
        {
            return cbor.GetByteString();
        }
        catch (Exception e)
        {
            return new CborIsNotAByteStringError(cbor.ToString(), e);
        }
    }

    public static Validation<CBORObject> Decode(byte[] bytes)
    {
        try
        {
            return CBORObject.DecodeFromBytes(bytes);
        }
        catch (Exception e)
        {
            return new CborDecodingError(e);
        }
    }
}
