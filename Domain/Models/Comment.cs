using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = true;

        // Relaciones
        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

        public string AuthorId { get; set; } = string.Empty;

        public User Author { get; set; } = null!;
    }
}