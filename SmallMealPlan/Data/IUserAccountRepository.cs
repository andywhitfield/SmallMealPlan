using System.Security.Claims;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IUserAccountRepository
    {
        Task CreateNewUserAsync(ClaimsPrincipal user);
        Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
        Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user);
    }
}