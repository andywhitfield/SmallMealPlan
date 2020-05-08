using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmallMealPlan.Data
{
    // used by the migrations tool only
    public class SqliteDataContextFactory : IDesignTimeDbContextFactory<SqliteDataContext>
    {
        public SqliteDataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqliteDataContext>();
            optionsBuilder.UseSqlite("Data Source=SmallMealPlan.Web/smallmealplan.db");
            return new SqliteDataContext(optionsBuilder.Options);
        }
    }
}