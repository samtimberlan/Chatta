using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Chatta.Core.Entities;
using Chatta.Core.Interfaces;
using Chatta.Infrastructure.DataContext;
using Chatta.Infrastructure.Extensions;
using Chatta.Infrastructure.ResultDTOs;
using Chatta.Infrastructure.Services;
using Chatta.Web.Authorization;

namespace Chatta.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly ChattaDbContext _context;
        private readonly IChattaRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly IUserManagerService _userManagerService;
        private readonly ILogger<PostsController> _logger;
        private readonly IAuthorizationService _authorizationService;

        public PostsController(ChattaDbContext context, IChattaRepository repository, IDistributedCache cache, IUserManagerService userManagerService, ILogger<PostsController> logger, IAuthorizationService authorizationService)
        {
            _context = context;
            _repository = repository;
            _cache = cache;
            _userManagerService = userManagerService;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check cache for posts. If it does not exist and new item has been added as determined by postCount, retrieve from db
                string cacheKey = "AllPosts";
                var posts = await _cache.GetCacheValueAsync<IList<Post>>(cacheKey);
                int postCount = await _repository.GetPostsCountAsync();

                if (posts == null || posts.Count != postCount)
                {
                    posts = await _repository.GetAllPostsByDescendingOrderAsync();
                    await _cache.SetCacheValueAsync(cacheKey, posts);
                }
                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var post = await _repository.GetPostByIdAsync(id);

                if (post == null)
                {
                    return NotFound();
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    post.Id = Guid.NewGuid();
                    Poster poster = await _repository.GetPosterByIdentityIdAsync();
                    if (poster == null)
                    {
                        _logger.LogInformation("Poster is null");
                        return View(post);
                    }
                    post.Poster = poster;
                    post.DateTimePosted = DateTime.UtcNow;
                    await _repository.AddAsync<Post>(post);
                    await _repository.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var post = await _repository.GetPostByIdAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var post = await _repository.GetPostByIdAsync(id);

                var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                User, post,
                                                PostOperations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                
                _repository.Delete<Post>(post);
                await _repository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST: Posts/Details/{id}
        [HttpPost("Posts/Details/{Id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(Guid id, [Bind("Content")] Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Post post = await _repository.GetPostByIdAsync(id);
                    comment.Id = Guid.NewGuid();
                    comment.Post = post;
                    post.Comments.Add(comment);
                    await _repository.AddAsync<Comment>(comment);
                    await _repository.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                return View("Posts/Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST: Posts/Details/{id}
        [HttpPost("Posts/Details/{Id}/Like")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(Guid id, [Bind("IsLikeActive")] Like like)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Post post = await _repository.GetPostByIdAsync(id);
                    FindOrCreateLikeResult result = await _repository.FindOrCreateLikeAsync(post, like);
                    await _repository.SaveChangesAsync();
                    if (!result.Success)
                    {
                        return View("Posts/Details");
                    }

                    return RedirectToAction(nameof(Index));
                }
                return View("Posts/Details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
