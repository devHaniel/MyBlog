using Application.Interfaces;
using Ganss.Xss;

namespace Application.Services;

public sealed class ArticleContentSanitizer : IArticleContentSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public ArticleContentSanitizer()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.UnionWith(
        [
            "a", "blockquote", "br", "code", "div", "em", "h1", "h2", "h3", "h4", "hr",
            "i", "img", "li", "ol", "p", "pre", "span", "strong", "sub", "sup",
            "table", "tbody", "td", "tfoot", "th", "thead", "tr", "u", "ul"
        ]);

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.UnionWith(
        [
            "alt", "colspan", "href", "rowspan", "src", "style", "title"
        ]);

        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedCssProperties.UnionWith(
        [
            "background-color", "border", "border-radius", "color", "font-size",
            "font-style", "font-weight", "text-align", "text-decoration"
        ]);

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
    }

    public string Sanitize(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return _sanitizer.Sanitize(content);
    }
}
