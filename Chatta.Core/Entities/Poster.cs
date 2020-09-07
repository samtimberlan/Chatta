using Chatta.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Chatta.Core.Entities
{
    public class Poster : IUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public Guid IdentityUserId { get; set; }
        public IList<Like> Likes { get; set; }
        public IList<Post> Posts { get; set; }
    }
}