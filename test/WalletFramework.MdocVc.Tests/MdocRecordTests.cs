using FluentAssertions;
using Hyperledger.Aries.Storage;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using Xunit;

namespace WalletFramework.MdocVc.Tests;

public class MdocRecordTests
{
    [Fact]
    public void Can_Encode_To_Json()
    {
        var encodedMdoc = MdocLib.Tests.Samples.EncodedMdoc;
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow(new InvalidOperationException("Mdoc sample is corrupt"));
        var record = mdoc.ToRecord(Option<List<MdocDisplay>>.None);

        var sut = JObject.FromObject(record);

        sut[nameof(RecordBase.Id)]!.ToString().Should().Be(record.Id);
        sut[MdocRecordFun.MdocJsonKey]!.ToString().Should().Be(encodedMdoc);
    }

    [Fact]
    public void Can_Decode_From_Json()
    {
        var json = MdocVcSamples.MdocRecordJson;

        var sut = json.ToObject<MdocRecord>()!;

        sut.Mdoc.DocType.ToString().Should().Be(MdocLib.Tests.Samples.DocType);
    }
}
