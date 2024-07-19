using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Common;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib;

public struct ValidityInfo
{
    public DateTime Signed { get; }
    
    public DateTime ValidFrom { get; }
    
    public DateTime ValidUntil { get; }
    
    public Option<DateTime> ExpectedUpdate { get; }
    
    private ValidityInfo(
        DateTime signed,
        DateTime validFrom,
        DateTime validUntil,
        Option<DateTime> expectedUpdate)
    {
        Signed = signed;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        ExpectedUpdate = expectedUpdate;
    }
    
    private static ValidityInfo Create(DateTime signed, DateTime validFrom, DateTime validUntil, Option<DateTime> expectedUpdate) =>
        new(signed, validFrom, validUntil, expectedUpdate);

    public static Validation<ValidityInfo> ValidValidityInfo(CBORObject mso)
    {
        var validityInfo = mso.GetByLabel("validityInfo");

        var signed =
            from info in validityInfo
            from dateTime in info.GetByLabel("signed").OnSuccess(ParseDateTime)
            select dateTime;
            
        var validFrom =
            from info in validityInfo
            from dateTime in info.GetByLabel("validFrom").OnSuccess(ParseDateTime)
            select dateTime;
        
        var validUntil =
            from info in validityInfo
            from dateTime in info.GetByLabel("validUntil").OnSuccess(ParseDateTime)
            select dateTime;
        
        var expectedUpdate =
            from info in validityInfo
            let dateTime = info.GetByLabel("expectedUpdate").OnSuccess(ParseDateTime).ToOption()
            select dateTime;

        return 
            Valid(Create)
                .Apply(signed)
                .Apply(validFrom)
                .Apply(validUntil)
                .Apply(expectedUpdate);
    }

    private static Validation<DateTime> ParseDateTime(CBORObject cbor)
    {
        string str;
        try
        {
            str = cbor.AsString();
        }
        catch (Exception e)
        {
            return new CborIsNotATextStringError("DateTime", e);
        }

        if (string.IsNullOrEmpty(str))
        {
            return new CborValueIsNullOrEmptyError("DateTime");
        }
        else
        {
            try
            {
               return DateTime.Parse(str);
            }
            catch (Exception e)
            {
                return new InvalidDateTimeStringError(str, e);
            }
        }
    }
}
