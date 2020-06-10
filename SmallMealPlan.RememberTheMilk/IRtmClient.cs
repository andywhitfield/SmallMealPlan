using System.Threading.Tasks;
using SmallMealPlan.RememberTheMilk.Contracts;

namespace SmallMealPlan.RememberTheMilk
{
    public interface IRtmClient
    {
        Task<RtmAuthGetTokenResponse> GetTokenAsync(string frob);
        Task<RtmListsGetListResponse> GetListsAsync(string authToken);
    }
}