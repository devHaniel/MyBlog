using Application.DTOs;
using Domain.Models;

namespace Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByUserNameAsync(string userName);
    Task<UserDto> CreateAsync(CreateUserDto dto, string password);
    Task UpdateAsync(string id, UpdateUserDto dto);
    Task DeleteAsync(string id);
}
