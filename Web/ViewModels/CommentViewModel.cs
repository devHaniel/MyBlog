using Application.DTOs;

namespace Web.ViewModels;

public class CommentIndexViewModel
{
    public IEnumerable<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public int PostId { get; set; }
}

public class CommentDetailsViewModel
{
    public CommentDto Comment { get; set; } = new(
        0,
        string.Empty,
        DateTime.UtcNow,
        false,
        0,
        string.Empty,
        string.Empty);
}

public class EditCommentViewModel
{
    public int Id { get; set; }
    public UpdateCommentDto Dto { get; set; } = new(null, null);
    public int PostId { get; set; }
}

public class CreateCommentViewModel
{
    public int PostId { get; set; }
    public CreateCommentDto Dto { get; set; } = new(0, string.Empty, string.Empty, false);
}