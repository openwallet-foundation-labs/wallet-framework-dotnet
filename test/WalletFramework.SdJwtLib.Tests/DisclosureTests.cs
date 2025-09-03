using WalletFramework.SdJwtLib.Models;

namespace WalletFramework.SdJwtLib.Tests;

public class DisclosureTests
{
    private readonly string _serialisedDisclosure = "WyJfMjZiYzRMVC1hYzZxMktJNmNCVzVlcyIsImZhbWlseV9uYW1lIiwiTcO2Yml1cyJd";
    private readonly Disclosure _deserialisedDisclosure = 
        new("family_name", "Möbius") { Salt = "_26bc4LT-ac6q2KI6cBW5es" };

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CanSerializeDisclosure()
    {
        Assert.That(_serialisedDisclosure, Is.EqualTo(_deserialisedDisclosure.Serialize()));
    }

    [Test]
    public void CanDeserializeDisclosure()
    {
        Assert.That(_deserialisedDisclosure.Salt,  Is.EqualTo(Disclosure.Deserialize(_serialisedDisclosure).Salt));
        Assert.That(_deserialisedDisclosure.Name,  Is.EqualTo(Disclosure.Deserialize(_serialisedDisclosure).Name));
        Assert.That(_deserialisedDisclosure.Value,  Is.EqualTo(Disclosure.Deserialize(_serialisedDisclosure).Value.ToString()));
    }

    [Test]
    public void CanComputeHash()
    {
        string expectedHash = "TZjouOTrBKEwUNjNDs9yeMzBoQn8FFLPaJjRRmAtwrM";

        string actualHash = _deserialisedDisclosure.GetDigest();
        
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }
}
