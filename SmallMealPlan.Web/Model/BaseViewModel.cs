namespace SmallMealPlan.Web.Model
{
    public abstract class BaseViewModel
    {
        public SmpArea? SelectedArea { get; protected set; }

        public string PlannerCss => AppendSelected(SmpArea.Planner, "smp-planner");
        public string MealsCss => AppendSelected(SmpArea.Meals, "smp-meals");
        public string ShoppingListCss => AppendSelected(SmpArea.ShoppingList, "smp-shoppinglist");
        public string NotesCss => AppendSelected(SmpArea.Notes, "smp-notes");

        private string AppendSelected(SmpArea area, string css) => area == SelectedArea ? $"{css} smp-selected" : css;
    }
}