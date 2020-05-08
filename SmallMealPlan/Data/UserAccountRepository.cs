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
        private readonly SqliteDataContext context;
        private readonly ILogger<UserAccountRepository> logger;

        public UserAccountRepository(SqliteDataContext context, ILogger<UserAccountRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Task CreateNewUserAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            var newAccount = new UserAccount { AuthenticationUri = authenticationUri };

            context.UserAccounts.Add(newAccount);
            return context.SaveChangesAsync();
        }

        private string GetIdentifierFromPrincipal(ClaimsPrincipal user) => user?.FindFirstValue("sub");

        public Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user) => GetUserAccountOrNullAsync(user) ?? throw new ArgumentException($"No UserAccount for the user: {user}");

        private Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            if (string.IsNullOrWhiteSpace(authenticationUri))
                return null;

            return context.UserAccounts.FirstOrDefaultAsync(ua => ua.AuthenticationUri == authenticationUri);
        }

        public async Task SaveUserAccountAsync(UserAccount userAccount)
        {
            context.UserAccounts.Update(userAccount);
            await context.SaveChangesAsync();
        }
    }
}