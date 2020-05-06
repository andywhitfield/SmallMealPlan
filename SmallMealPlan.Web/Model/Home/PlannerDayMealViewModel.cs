using System.Collections.Generic;
using System.Linq;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerDayMealViewModel
    {
        public PlannerDayMealViewModel(int id) => Id = id;

        public int Id { get; }

        public string Name { get; set; }

        public bool HasIngredients => Ingredients?.Any() ?? false;
        public IEnumerable<string> Ingredients { get; set; }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
        public string Notes { get; set; }
    }
}