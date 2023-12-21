using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface IUserAccountRepository
{
    Task<UserAccount> CreateNewUserAsync(string email, byte[] credentialId, byte[] publicKey, byte[] userHandle);
    Task<UserAccount?> GetUserAccountAsync(int userAccountId);
    Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
    Task<UserAccount?> GetUserAccountByEmailAsync(string email);
    Task UpdateAsync(UserAccount user);
    IAsyncEnumerable<UserAccountCredential> GetUserAccountCredentialsAsync(UserAccount user);
    Task<UserAccountCredential?> GetUserAccountCredentialsByUserHandleAsync(byte[] userHandle);
    Task SetSignatureCountAsync(UserAccountCredential userAccountCredential, uint signatureCount);
}