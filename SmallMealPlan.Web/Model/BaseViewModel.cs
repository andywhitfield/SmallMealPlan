using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model
{
    public abstract class BaseViewModel
    {
        public const string ShortDateFormat = "ddd dd MMM yyyy";

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

        public string AppendCss(bool appendCondition, string css, string appendCss) => appendCondition ? $"{css} {appendCss}" : css;
        private string AppendSelected(SmpArea area, string css) => AppendCss(area == SelectedArea, css, "smp-selected");
    }
}