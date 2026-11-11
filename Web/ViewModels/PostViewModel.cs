using Application.DTOs;

namespace Web.ViewModels;

public class PostIndexViewModel
{
    public IEnumerable<PostDto> Posts { get; set; } = new List<PostDto>();
}

public class PostDetailsViewModel
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
}

public class CreatePostViewModel
{
    public CreatePostDto Dto { get; set; } = new(
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        null,
        false,
        string.Empty,
        new List<ArticleDto>(),
        new List<int>());

    public IEnumerable<CategoryDto> AvailableCategories { get; set; } = new List<CategoryDto>();
    public IEnumerable<UserDto> AvailableAuthors { get; set; } = new List<UserDto>();
}

public class EditPostViewModel
{
    public int Id { get; set; }
    public UpdatePostDto Dto { get; set; } = new(null, null, null, null, null, null, null, null);
    public IEnumerable<CategoryDto> AvailableCategories { get; set; } = new List<CategoryDto>();
}