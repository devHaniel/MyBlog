using Application.DTOs;

namespace Application.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetAllAsync();
    Task<PostDto?> GetByIdAsync(int id);
    Task<PostDto?> GetBySlugAsync(string slug);
    Task<PostDto> CreateAsync(string authorId, CreatePostDto dto);
    Task UpdateAsync(string authorId, int id, UpdatePostDto dto);
    Task DeleteAsync(string authorId, int id);
}
