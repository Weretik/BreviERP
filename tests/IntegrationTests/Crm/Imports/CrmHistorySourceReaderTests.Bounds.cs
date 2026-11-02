using System.Text.Json;
using Crm.Application.Imports.History.Contracts;

namespace IntegrationTests.Crm.Imports;

public sealed partial class CrmHistorySourceReaderTests
{
    [Test]
    public async Task Reader_BoundsReverseMembershipAndExplicitlyMarksOversizedQuoteGraph()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["quotes"].Add(new { id="q1", current_version_number=1, created_by_email="source@example.test", current_responsible_email="source@example.test" });
        for (var i=1; i<=CrmHistoryLimits.MaximumRowReferences+1; i++)
            fixture.Rows["quote_versions"].Add(new { id="v"+i, quote_id="q1", version_number=i, snapshot_json=JsonSerializer.Serialize(new{items=Array.Empty<object>()}),changes_json="[]",created_by_email="source@example.test" });
        fixture.Write();
        var request = await reader.ReadAsync(fixture.FreezePath,fixture.Root);
        var quote = request.Snapshot.Rows.Single(x=>x.SourceId=="q1");
        Assert.That(quote.References.Count,Is.LessThanOrEqualTo(CrmHistoryLimits.MaximumRowReferences));
        Assert.That(quote.Issues,Does.Contain("graph_limit_exceeded"));
        Assert.That(JsonSerializer.Serialize(request),Does.Not.Contain("source@example.test"));
    }
    [Test]
    public async Task Reader_StopsBeforeNormalizingMoreThanTheSelectedRowBudget()
    {
        using var fixture=new CrmHistorySourceFixture();
        for(var i=0;i<CrmHistoryLimits.MaximumRows+1;i++) fixture.Rows["counterparties"].Add(new{id="synthetic_row"});
        fixture.Write();
        var error=await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(fixture.FreezePath,fixture.Root));
        Assert.That(error!.Message,Is.EqualTo("crm_history_source_invalid"));
    }

    [Test]
    public async Task Reader_StopsDuplicateRootAmplificationAtTheGlobalReferenceBudget()
    {
        using var fixture=new CrmHistorySourceFixture();
        var rootCount=CrmHistoryLimits.MaximumReferences/CrmHistoryLimits.MaximumRowReferences+1;
        for(var i=0;i<rootCount;i++) fixture.Rows["quotes"].Add(new{id="q1",current_version_number=1,created_by_email="source@example.test",current_responsible_email="source@example.test"});
        for(var i=1;i<=CrmHistoryLimits.MaximumRowReferences;i++) fixture.Rows["quote_versions"].Add(new{id="v"+i,quote_id="q1",version_number=i,snapshot_json=JsonSerializer.Serialize(new{items=Array.Empty<object>()}),changes_json="[]",created_by_email="source@example.test"});
        fixture.Write();
        var error=await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(fixture.FreezePath,fixture.Root));
        Assert.That(error!.Message,Is.EqualTo("crm_history_source_invalid"));
    }

}
