namespace Application.Interfaces;

public interface IArticleContentSanitizer
{
    string Sanitize(string content);
}
