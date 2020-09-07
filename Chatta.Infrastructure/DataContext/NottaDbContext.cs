using Microsoft.EntityFrameworkCore;
using Chatta.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatta.Infrastructure.DataContext
{
    public class ChattaDbContext : DbContext
    {
        public ChattaDbContext(DbContextOptions<ChattaDbContext> options) : base(options)
        {

        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Poster> Posters { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
