using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Blog.MVC.Helpers;

public static class SlugHelper
{
    public static string Generate(string? value, string fallback = "item")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return $"{fallback}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormKD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (ch is ' ' or '_' or '-')
            {
                builder.Append('-');
            }
        }

        var slug = Regex.Replace(builder.ToString(), "-{2,}", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"{fallback}-{DateTime.UtcNow:yyyyMMddHHmmss}" : slug;
    }
}
