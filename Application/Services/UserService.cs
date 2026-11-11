using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _userManager.Users
                .AsNoTracking()
                .OrderBy(user => user.UserName)
                .Select(user => new UserDto(
                    user.Id,
                    user.FullName,
                    user.UserName ?? string.Empty,
                    user.Email,
                    user.CreatedAt,
                    user.Posts.Count,
                    user.Comments.Count))
                .ToListAsync();
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, string password)
        {
            if(dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var user = new User{
                FullName = dto.FullName,
                UserName = dto.UserName ?? string.Empty,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            EnsureSucceeded(result);
            
            var newDto = new UserDto(
                user.Id,
                user.FullName,
                user.UserName ?? string.Empty,
                user.Email,
                user.CreatedAt,
                user.Posts.Count,
                user.Comments.Count
            );

            return newDto;
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found.", nameof(id));
            }

            var result = await _userManager.DeleteAsync(user);
            EnsureSucceeded(result);
        }

        public Task<UserDto?> GetByIdAsync(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            }

            return FindByIdAsync(id);
        }

        public async Task<UserDto?> GetByUserNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            return user is not null
                ? new UserDto(
                    user.Id,
                    user.FullName,
                    user.UserName ?? string.Empty,
                    user.Email,
                    user.CreatedAt,
                    user.Posts.Count,
                    user.Comments.Count)
                : null;
        }

        public async Task UpdateAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found.", nameof(id));
            }

            user.FullName = dto.FullName ?? user.FullName;
            user.UserName = dto.UserName ?? user.UserName;
            user.Email = dto.Email ?? user.Email;

            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            EnsureSucceeded(result);
        }

        private async Task<UserDto?> FindByIdAsync(string id)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(user => user.Id == id)
                .Select(user => new UserDto(
                    user.Id,
                    user.FullName,
                    user.UserName ?? string.Empty,
                    user.Email,
                    user.CreatedAt,
                    user.Posts.Count,
                    user.Comments.Count))
                .SingleOrDefaultAsync();

            return user;
        }

        private static void EnsureSucceeded(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }
    }
}