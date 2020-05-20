using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Meals
{
    public class EditMealViewModel : BaseViewModel
    {
        public EditMealViewModel(HttpContext context, int mealId) : base(context)
        {
            MealId = mealId;
            SelectedArea = SmpArea.Meals;
        }

        public int MealId { get; }
        public string Name { get; set; }
        public string Ingredients { get; set; }
        public string Notes { get; set; }
    }
}