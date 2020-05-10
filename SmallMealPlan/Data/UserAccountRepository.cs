using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<UserAccountRepository> _logger;

        public UserAccountRepository(SqliteDataContext context, ILogger<UserAccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task CreateNewUserAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            var newAccount = new UserAccount { AuthenticationUri = authenticationUri };

            _context.UserAccounts.Add(newAccount);
            return _context.SaveChangesAsync();
        }

        private string GetIdentifierFromPrincipal(ClaimsPrincipal user) => user?.FindFirstValue("sub");

        public Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user) => GetUserAccountOrNullAsync(user) ?? throw new ArgumentException($"No UserAccount for the user: {user}");

        public Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            if (string.IsNullOrWhiteSpace(authenticationUri))
                return null;

            return _context.UserAccounts.FirstOrDefaultAsync(ua => ua.AuthenticationUri == authenticationUri);
        }

        public async Task SaveUserAccountAsync(UserAccount userAccount)
        {
            _context.UserAccounts.Update(userAccount);
            await _context.SaveChangesAsync();
        }
    }
}