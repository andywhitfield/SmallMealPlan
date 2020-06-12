using System.Threading.Tasks;
using SmallMealPlan.RememberTheMilk.Contracts;

namespace SmallMealPlan.RememberTheMilk
{
    public interface IRtmClient
    {
        Task<RtmAuth> GetTokenAsync(string frob);
        Task<RtmLists> GetListsAsync(string authToken);
    }
}