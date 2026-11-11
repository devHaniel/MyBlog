using Application.DTOs;

namespace Web.Models;

public class HomeIndexViewModel
{
    public string Title { get; set; } = "Blog";
    public List<PostDto> Posts { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
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
    public List<CategoryDto> Categories { get; set; } = new();
}
