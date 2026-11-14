using System.Globalization;
using System.Net;
using System.Text;
using Production.Domain.Orders;

namespace Production.Application.Features.Orders.Commands.SendProductionOrderTelegram;

/// <summary>The text of the message to the workshop group, in Telegram HTML, within its 4096-character limit.</summary>
public static class ProductionTelegramMessage
{
    public const int MaxLength = 4000;
    private const int MaxFiles = 20;

    public static string Build(ProductionOrder order, IReadOnlyList<ProductionOrderFile> files, string link)
    {
        var footer = new StringBuilder();
        var shownFiles = files.Take(MaxFiles).ToList();
        if (shownFiles.Count > 0)
        {
            footer.Append("\n\nФайли:");
            foreach (var file in shownFiles)
                footer.Append("\n• <a href=\"").Append(Escape(file.Url)).Append("\">").Append(Escape(file.Name)).Append("</a>");
            if (files.Count > shownFiles.Count)
                footer.Append("\nЩе файлів: ").Append(files.Count - shownFiles.Count);
        }

        footer.Append("\n\n<a href=\"").Append(Escape(link)).Append("\">Відкрити завдання</a>");

        var text = new StringBuilder();
        text.Append("<b>Нове завдання № ").Append(order.Number).Append("</b>")
            .Append("\nКлієнт: ").Append(Escape(order.ClientName))
            .Append("\nМенеджер: ").Append(Escape(order.ManagerName));
        if (order.Comment is { } comment) text.Append("\nКоментар: ").Append(Escape(comment));

        var items = order.Items.OrderBy(x => x.Position).ToList();
        for (var index = 0; index < items.Count; index++)
        {
            var line = ItemLine(items[index]);
            if (text.Length + line.Length + footer.Length > MaxLength - 60)
            {
                text.Append("\n\nЩе позицій: ").Append(items.Count - index).Append(". Повний склад у завданні.");
                break;
            }

            text.Append(line);
        }

        return text.Append(footer).ToString();
    }

    private static string ItemLine(ProductionItem item)
    {
        var line = new StringBuilder("\n\n");
        line.Append(item.Position).Append(". ").Append(Escape(item.ModelName));
        if (item.ModelCode is { } code) line.Append(" (").Append(Escape(code)).Append(')');
        if (item.FabricName is { } fabric) line.Append("\nТканина: ").Append(Escape(fabric));
        line.Append("\nКолір: ").Append(Escape(string.Join(" + ", new[] { item.MainColor }.Concat(item.AdditionalColors))));
        line.Append("\nКількість: ").Append(item.Quantity).Append(" шт. · Готово до ")
            .Append(item.DueDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
        if (item.Applications.Count > 0)
            line.Append("\nНанесення: ").Append(item.Applications.Count);
        return line.ToString();
    }

    private static string Escape(string value) => WebUtility.HtmlEncode(value);
}
