namespace SmallMealPlan.Web.Model.Home;

public record MealApiModel(int Id, DateOnly Date, string DateDesc, string Title, string[] Ingredients, string Notes, string DateNotes);
