using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class UserAccountRepository(ILogger<UserAccountRepository> logger, SqliteDataContext context) : IUserAccountRepository
{
    public async Task<UserAccount> CreateNewUserAsync(string email, byte[] credentialId, byte[] publicKey, byte[] userHandle)
    {
        var user = context.UserAccounts.Add(new() { Email = email });
        context.UserAccountCredentials!.Add(new()
        {
            UserAccount = user.Entity,
            CredentialId = credentialId,
            PublicKey = publicKey,
            UserHandle = userHandle
        });
        await context.SaveChangesAsync();
        return user.Entity;
    }

    public async Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user)
    {
        var email = user?.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation($"Could not get account identifier from principal: {user?.Identity?.Name}:[{string.Join(',', (user?.Claims ?? Enumerable.Empty<Claim>()).Select(c => $"{c.Type}={c.Value}"))}]");
            throw new ArgumentException($"No UserAccount for the user: {user?.Identity?.Name}");
        }

        return (await context.UserAccounts.FirstOrDefaultAsync(ua => ua.Email == email && ua.DeletedDateTime == null))
            ?? throw new ArgumentException($"No UserAccount for the user: {user?.Identity?.Name}");
    }

    public Task<UserAccount?> GetUserAccountAsync(int userAccountId) =>
        context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserAccountId == userAccountId && ua.DeletedDateTime == null);

    public Task<UserAccount?> GetUserAccountByEmailAsync(string email)
        => context.UserAccounts!.FirstOrDefaultAsync(a => a.DeletedDateTime == null && a.Email == email);

    public Task UpdateAsync(UserAccount user)
    {
        user.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public IAsyncEnumerable<UserAccountCredential> GetUserAccountCredentialsAsync(UserAccount user)
        => context.UserAccountCredentials!.Where(uac => uac.DeletedDateTime == null && uac.UserAccountId == user.UserAccountId).AsAsyncEnumerable();

    public Task<UserAccountCredential?> GetUserAccountCredentialsByUserHandleAsync(byte[] userHandle)
        => context.UserAccountCredentials!.FirstOrDefaultAsync(uac => uac.DeletedDateTime == null && uac.UserHandle.SequenceEqual(userHandle));

    public Task SetSignatureCountAsync(UserAccountCredential userAccountCredential, uint signatureCount)
    {
        userAccountCredential.SignatureCount = signatureCount;
        return context.SaveChangesAsync();
    }
}