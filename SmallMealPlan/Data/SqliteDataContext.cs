using Microsoft.EntityFrameworkCore;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class SqliteDataContext(DbContextOptions<SqliteDataContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserAccountCredential> UserAccountCredentials { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<MealIngredient> MealIngredients { get; set; }
    public DbSet<PlannerMeal> PlannerMeals { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
}