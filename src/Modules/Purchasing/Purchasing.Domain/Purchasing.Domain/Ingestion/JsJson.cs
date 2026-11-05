using System.Text;

namespace Purchasing.Domain.Ingestion;

/// <summary>
/// Writes strings and flat string objects byte-for-byte like JavaScript JSON.stringify, because the old Calculator
/// hashed that text (revision ids, checked-document hashes).
/// </summary>
public static class JsJson
{
    public static string Object(IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        var builder = new StringBuilder("{");
        for (var i = 0; i < fields.Count; i++)
        {
            if (i > 0) builder.Append(',');
            AppendString(builder, fields[i].Key);
            builder.Append(':');
            AppendString(builder, fields[i].Value);
        }

        return builder.Append('}').ToString();
    }

    public static string String(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        AppendString(builder, value);
        return builder.ToString();
    }

    public static void AppendString(StringBuilder builder, string value)
    {
        builder.Append('"');
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            switch (c)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (c < 0x20 || IsLoneSurrogate(value, i))
                        builder.Append("\\u").Append(((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture));
                    else
                        builder.Append(c);
                    break;
            }
        }

        builder.Append('"');
    }

    private static bool IsLoneSurrogate(string value, int i)
    {
        var c = value[i];
        if (char.IsHighSurrogate(c)) return i + 1 >= value.Length || !char.IsLowSurrogate(value[i + 1]);
        if (char.IsLowSurrogate(c)) return i == 0 || !char.IsHighSurrogate(value[i - 1]);
        return false;
    }
}
