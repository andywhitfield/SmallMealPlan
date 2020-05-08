using Microsoft.EntityFrameworkCore;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class SqliteDataContext : DbContext
    {
        public SqliteDataContext(DbContextOptions<SqliteDataContext> options) : base(options) { }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<MealIngredient> MealIngredients { get; set; }
        public DbSet<PlannerMeal> PlannerMeals { get; set; }
    }
}