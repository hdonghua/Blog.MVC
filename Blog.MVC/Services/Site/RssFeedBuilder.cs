using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.Services.Site;

public static class RssFeedBuilder
{
    public static byte[] Build(
        string siteTitle,
        string siteDescription,
        string siteUrl,
        string feedUrl,
        string? managingEditor,
        IReadOnlyList<ArticleListDto> articles,
        Func<string, string> articleUrlBuilder)
    {
        var siteUri = new Uri(siteUrl);
        var feedUri = new Uri(feedUrl);
        var latestUpdate = articles.Count > 0
            ? new DateTimeOffset((articles.Max(x => x.PublishedTime ?? x.CreationTime)).ToUniversalTime())
            : DateTimeOffset.UtcNow;

        var feed = new SyndicationFeed(
            siteTitle,
            siteDescription,
            siteUri,
            feedUrl,
            latestUpdate)
        {
            Language = "zh-CN",
            LastUpdatedTime = latestUpdate,
            Generator = "Blog.MVC"
        };

        feed.Links.Add(SyndicationLink.CreateSelfLink(feedUri));

        if (!string.IsNullOrWhiteSpace(managingEditor))
        {
            feed.ElementExtensions.Add("managingEditor", string.Empty, managingEditor);
        }

        feed.Items = articles.Select(article => CreateItem(article, articleUrlBuilder)).ToList();

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            NewLineHandling = NewLineHandling.Entitize,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            var formatter = new Rss20FeedFormatter(feed, false);
            formatter.WriteTo(writer);
            writer.Flush();
        }

        return stream.ToArray();
    }

    private static SyndicationItem CreateItem(ArticleListDto article, Func<string, string> articleUrlBuilder)
    {
        var articleUrl = articleUrlBuilder(article.Slug);
        var publishedAt = new DateTimeOffset((article.PublishedTime ?? article.CreationTime).ToUniversalTime());
        var description = BuildItemDescription(article, articleUrl);

        var item = new SyndicationItem(
            article.Title,
            new TextSyndicationContent(description, TextSyndicationContentKind.Html),
            new Uri(articleUrl),
            articleUrl,
            publishedAt)
        {
            PublishDate = publishedAt,
            LastUpdatedTime = publishedAt
        };

        if (!string.IsNullOrWhiteSpace(article.AuthorName))
        {
            item.Authors.Add(new SyndicationPerson(null, article.AuthorName, articleUrl));
        }

        if (!string.IsNullOrWhiteSpace(article.CategoryName))
        {
            item.Categories.Add(new SyndicationCategory(article.CategoryName));
        }

        return item;
    }

    private static string BuildItemDescription(ArticleListDto article, string articleUrl)
    {
        var encodedUrl = WebUtility.HtmlEncode(articleUrl);
        var readMoreLink = $"<a href=\"{encodedUrl}\">查看全文</a>";

        if (string.IsNullOrWhiteSpace(article.Summary))
        {
            return readMoreLink;
        }

        return $"{WebUtility.HtmlEncode(article.Summary.Trim())} {readMoreLink}";
    }
}
