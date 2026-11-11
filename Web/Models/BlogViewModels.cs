using Application.DTOs;

namespace Web.Models;

public class HomeIndexViewModel
{
    public string Title { get; set; } = "Blog";
    public List<PostDto> Posts { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class HomePostCardViewModel
{
    public PostDto Post { get; set; } = null!;
    public List<CategoryDto> Categories { get; set; } = new();

    public int ReadingMinutes
    {
        get
        {
            var content = string.Join(
                " ",
                new[] { Post.Content }
                    .Concat(Post.Articles.Select(article => article.Content))
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            var wordCount = content.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries).Length;

            return Math.Max(1, (int)Math.Ceiling(wordCount / 200d));
        }
    }
}

public class PostDetailViewModel
{
    public PostDto Post { get; set; } = new(
        0,
        string.Empty,
        string.Empty,
        string.Empty,
        new List<ArticleDto>(),
        null,
        null,
        false,
        DateTime.UtcNow,
        null,
        null,
        string.Empty,
        string.Empty,
        new List<int>(),
        0);

    public List<CommentDto> Comments { get; set; } = new();
    public List<CommentThreadViewModel> CommentThreads { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public string CurrentUserName { get; set; } = string.Empty;
    public string CurrentUserEmail { get; set; } = string.Empty;
}

public class CommentThreadViewModel
{
    public CommentDto Comment { get; set; } = null!;
    public List<CommentThreadViewModel> Replies { get; set; } = new();
}
