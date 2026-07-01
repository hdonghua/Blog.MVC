using System.Text;
using System.Xml;
using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.Helpers;

public static class SeoHelper
{
    public const int DescriptionMaxLength = 160;

    public static string ResolveBaseUrl(IConfiguration configuration, HttpRequest request)
    {
        var configured = configuration["Site:BaseUrl"]?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return $"{request.Scheme}://{request.Host}";
    }

    public static string Truncate(string? text, int maxLength = DescriptionMaxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength].TrimEnd() + "…";
    }

    public static string BuildDocumentTitle(string siteTitle, string siteDescription, string? pageTitle, bool isHome)
    {
        if (isHome)
        {
            return $"{siteTitle} - {siteDescription}";
        }

        return string.IsNullOrWhiteSpace(pageTitle) ? siteTitle : $"{pageTitle} - {siteTitle}";
    }

    public static byte[] BuildSitemap(string baseUrl, IReadOnlyList<ArticleListDto> articles)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            WriteSitemapUrl(writer, $"{baseUrl}/", DateTime.UtcNow, "daily", "1.0");
            WriteSitemapUrl(writer, $"{baseUrl}/Blog/Timeline", DateTime.UtcNow, "daily", "0.8");
            WriteSitemapUrl(writer, $"{baseUrl}/Home/About", DateTime.UtcNow, "monthly", "0.6");

            foreach (var article in articles)
            {
                var lastModified = article.PublishedTime ?? article.CreationTime;
                WriteSitemapUrl(
                    writer,
                    $"{baseUrl}/Blog/Detail/{article.Slug}",
                    lastModified,
                    "weekly",
                    "0.7");
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return stream.ToArray();
    }

    private static void WriteSitemapUrl(
        XmlWriter writer,
        string location,
        DateTime lastModified,
        string changeFrequency,
        string priority)
    {
        writer.WriteStartElement("url");
        writer.WriteElementString("loc", location);
        writer.WriteElementString("lastmod", lastModified.ToUniversalTime().ToString("yyyy-MM-dd"));
        writer.WriteElementString("changefreq", changeFrequency);
        writer.WriteElementString("priority", priority);
        writer.WriteEndElement();
    }
}
