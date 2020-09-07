using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Chatta.Infrastructure.ResultDTOs;
using System;
using System.Threading.Tasks;

namespace Chatta.Infrastructure.Services
{
    public interface IUserManagerService
    {
        Task<UserManagerServiceResult> GetCurrentUserIdAsync();
    }
    public class UserManagerService : IUserManagerService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;

        public UserManagerService(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _httpContext = httpContext;
        }
        public async Task<UserManagerServiceResult> GetCurrentUserIdAsync()
        {
            IdentityUser user = await GetCurrentUserAsync();
            if (user == null)
            {
                return new UserManagerServiceResult
                {
                    Success = false
                };
            }
            else
            {
                Guid userIdGuid;
                Guid.TryParse(user?.Id.ToCharArray(), out userIdGuid);
                return new UserManagerServiceResult
                {
                    Id = userIdGuid,
                    Success = true
                };
            }
        }

        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_httpContext.HttpContext.User);
        }
    }
}
