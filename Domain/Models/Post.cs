using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Post
    {
        public int Id {get; set;}
        public string Title {get; set;} = string.Empty;
        public string Slug {get; set;} = string.Empty;
        public string Content {get; set;} = string.Empty;
        public string? Excerpt { get; set; }
        public string? CoverImage { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Relaciones
        public string AuthorId { get; set; } = string.Empty;

        public User Author { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
        
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}