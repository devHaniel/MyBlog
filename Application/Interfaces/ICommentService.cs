using Application.DTOs;

namespace Application.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByPostAsync(int postId);
    Task<CommentDto?> GetByIdAsync(int id);
    Task<CommentDto> CreateAsync(CreateCommentDto dto);
    Task UpdateAsync(int id, UpdateCommentDto dto);
    Task DeleteAsync(int id);
}
