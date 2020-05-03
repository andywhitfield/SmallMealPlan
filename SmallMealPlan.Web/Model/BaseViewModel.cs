using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model
{
    public abstract class BaseViewModel
    {
        protected BaseViewModel(HttpContext context)
        {
            IsLoggedIn = context.User?.Identity?.IsAuthenticated ?? false;
        }

        public SmpArea? SelectedArea { get; protected set; }

        public string PlannerCss => AppendSelected(SmpArea.Planner, "smp-planner");
        public string MealsCss => AppendSelected(SmpArea.Meals, "smp-meals");
        public string ShoppingListCss => AppendSelected(SmpArea.ShoppingList, "smp-shoppinglist");
        public string NotesCss => AppendSelected(SmpArea.Notes, "smp-notes");

        public bool IsLoggedIn { get; }

        private string AppendSelected(SmpArea area, string css) => area == SelectedArea ? $"{css} smp-selected" : css;
    }
}