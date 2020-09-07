using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Chatta.Core.Entities;
using Chatta.Core.Interfaces;
using Chatta.Infrastructure.DataContext;
using Chatta.Infrastructure.ResultDTOs;
using Chatta.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatta.Infrastructure.Repositories
{
    public class ChattaRepository : IChattaRepository
    {
        private readonly ILogger _logger;
        private readonly ChattaDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserManagerService _userManageService;

        public ChattaRepository(ILogger<ChattaRepository> logger, ChattaDbContext context, IHttpContextAccessor httpContext, IUserManagerService userManagerService)
        {
            _logger = logger;
            _context = context;
            _httpContext = httpContext;
            _userManageService = userManagerService;
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            _logger.LogInformation($"Adding entity of type {entity.GetType()} to the Db context");
            await _context.AddAsync<T>(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _logger.LogInformation($"Deleting entity of type {entity.GetType()} from the Db context");
            //return await Task.Factory.StartNew<T>(_context.Remove<T>(entity)).Result;
            _context.Remove<T>(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        #region Posts
        public async Task<IList<Post>> GetAllPostsByDescendingOrderAsync()
        {
            _logger.LogInformation($"Retrieving all posts");
            return await _context.Posts
                .Include(post => post.Comments)
                .Include(post => post.Likes)
                .OrderByDescending(post => post.DateTimePosted)
                .ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid? id)
        {
            _logger.LogInformation($"Reading entity of with {id} from the Db context");
            return await _context.Posts
                .Include(post => post.Comments)
                .Include(post => post.Likes)
                .Include(post => post.Poster)
                .FirstOrDefaultAsync(post => post.Id == id);
        }

        public async Task<int> GetPostsCountAsync()
        {
            return await _context.Posts.CountAsync();
        }
        #endregion

        #region Poster
        public async Task SetupPosterProfileAsync(Guid identityUserId, string userName)
        {
                var poster = new Poster
                {
                    Id = Guid.NewGuid(),
                    DateCreated = DateTime.UtcNow,
                    Name = userName,
                    IdentityUserId = identityUserId
                };
                await this.AddAsync<Poster>(poster);
                await this.SaveChangesAsync();
        }

        public async Task<Poster> GetPosterByIdentityIdAsync()
        {
            UserManagerServiceResult result = await _userManageService.GetCurrentUserIdAsync();
            if (!result.Success)
            {
                return null;
            }
            else
            {
                _logger.LogInformation($"Retrieving poster with Id: {result.Id}");
                return await _context.Posters.FirstOrDefaultAsync(poster => poster.IdentityUserId == result.Id);
            }
            
        }
        #endregion

        #region Likes
        public async Task<FindOrCreateLikeResult> FindOrCreateLikeAsync(Post post, Like like)
        {
            Like selectedLike;

            Poster poster = await this.GetPosterByIdentityIdAsync();

            if (poster == null)
            {
                _logger.LogError("Poster is null");
                return new FindOrCreateLikeResult
                {
                    Success = false
                };
            }

            try
            {
                selectedLike = await _context.Likes.FirstOrDefaultAsync(like => like.Id == like.Id);
                // Toggle Like on or off
                selectedLike.IsLikeActive = !selectedLike.IsLikeActive;
                try
                {
                    _context.Update(post);
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
            }
            catch (Exception)
            {

                selectedLike = new Like
                {
                    Id = Guid.NewGuid(),
                    IsLikeActive = true,
                    User = poster
                };
                post.Likes.Add(selectedLike);
                await this.AddAsync<Like>(selectedLike);
            }
            return new FindOrCreateLikeResult
            {
                Success = true,
                LikeObject = like
            };
        }
        #endregion

    }
}
