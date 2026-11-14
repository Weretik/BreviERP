using System.Text.Json;
using Content.Domain.Articles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

namespace UnitTests.Content;

/// <summary>The API and the stored texts write block kinds by name, as the contract says.</summary>
public sealed class ArticleBlockJsonTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Test]
    public void Block_kind_is_written_by_name()
    {
        var json = JsonSerializer.Serialize(new ArticleBlock(ArticleBlockKind.Paragraph, "Текст", null, null), Web);

        Assert.That(json, Does.Contain("\"kind\":\"Paragraph\""));
    }

    [Test]
    public void Old_site_block_reads_kind_by_name()
    {
        var block = JsonSerializer.Deserialize<OldSiteArticleBlock>(
            """{"kind":"Image","imageUrl":"https://brevi.com.ua/image/a.jpg","alt":"Цех"}""", Web);

        Assert.That(block!.Kind, Is.EqualTo(ArticleBlockKind.Image));
    }

    [Test]
    public void Stored_number_still_reads()
    {
        var block = JsonSerializer.Deserialize<ArticleBlock>("""{"kind":3,"items":["а"]}""", Web);

        Assert.That(block!.Kind, Is.EqualTo(ArticleBlockKind.List));
    }
}
