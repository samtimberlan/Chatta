using System;
using System.Collections.Generic;

namespace Chatta.Core.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime DateTimePosted { get; set; }
        public virtual IList<Comment> Comments { get; set; }
        public virtual IList<Like> Likes { get; set; }
        public virtual Poster Poster { get; set; }
    }
}
