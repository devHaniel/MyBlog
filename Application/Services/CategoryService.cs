using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _dbContext;

        public CategoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }   
        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var category = new Category
            {
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description
            };

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            return new CategoryDto(category.Id, category.Name, category.Slug, category.Description, 0);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new ArgumentException("Category not found.", nameof(id));
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.Description,
                    category.PostCategories.Count))
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(category => category.Id == id)
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.Description,
                    category.PostCategories.Count))
                .SingleOrDefaultAsync();
        }

        public async Task<CategoryDto?> GetBySlugAsync(string slug)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(category => category.Slug == slug)
                .Select(category => new CategoryDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.Description,
                    category.PostCategories.Count))
                .SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new ArgumentException("Category not found.", nameof(id));
            }

            category.Name = dto.Name == null ? category.Name : dto.Name;
            category.Slug = dto.Slug == null ? category.Slug : dto.Slug;
            category.Description = dto.Description == null ? category.Description : dto.Description;

            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }
    }
}