using System.Threading.Tasks;
using SmallMealPlan.RememberTheMilk.Contracts;

namespace SmallMealPlan.RememberTheMilk
{
    public interface IRtmClient
    {
        Task<RtmAuth> GetTokenAsync(string frob);
        Task<RtmLists> GetListsAsync(string authToken);
        Task<RtmTasks> GetTaskListsAsync(string authToken, string listId);
        Task<RtmList> AddTaskAsync(string authToken, string timeline, string listId, string itemToAddToList);
        Task<string> CreateTimelineAsync(string authToken);
    }
}