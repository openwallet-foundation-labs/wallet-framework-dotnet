using Newtonsoft.Json.Linq;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Tests.Examples;

namespace WalletFramework.SdJwtLib.Tests;

public class SdJwtDocTests
{
    [TestCase(typeof(Example1))]
    [TestCase(typeof(Example2))]
    [TestCase(typeof(Example3))]
    [TestCase(typeof(Example4A))]
    [TestCase(typeof(Example5))]
    [Test]
    public void CanParseExampleSdJwt(Type example)
    {
        var input = (BaseExample)Activator.CreateInstance(example)!;
        var doc = new SdJwtDoc(input.IssuedSdJwt);
        
        Assert.That(doc.UnsecuredPayload, Is.EqualTo(JObject.Parse(input.UnsecuredPayload)));
        Assert.That(doc.SecuredPayload, Is.EqualTo(JObject.Parse(input.SecuredPayload)));
        Assert.That(doc.Disclosures.Count, Is.EqualTo(input.NumberOfDisclosures));
    }
}
