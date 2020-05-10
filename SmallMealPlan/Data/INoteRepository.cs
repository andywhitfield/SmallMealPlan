using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface INoteRepository
    {
        Task<string> GetAsync(UserAccount user);
        Task AddOrUpdateAsync(UserAccount user, string noteText);
    }
}