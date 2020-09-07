using Chatta.Core.Entities;
using Chatta.Infrastructure.ResultDTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chatta.Core.Interfaces
{
    public interface IChattaRepository
    {
        Task AddAsync<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();
        Task<IList<Post>> GetAllPostsByDescendingOrderAsync();
        Task<Post> GetPostByIdAsync(Guid? id);
        Task<int> GetPostsCountAsync();
        Task SetupPosterProfileAsync(Guid id, string userName);
        Task<Poster> GetPosterByIdentityIdAsync();
        Task<FindOrCreateLikeResult> FindOrCreateLikeAsync(Post post, Like like);
    }
}
