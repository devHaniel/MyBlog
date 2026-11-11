using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Article
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string ContentType { get; set; } = "Paragraph";

        public int Order { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; } = null!;
    }
}