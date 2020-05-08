using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class SqliteDataContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqliteDataContext> _logger;

        public SqliteDataContext(IConfiguration configuration, ILogger<SqliteDataContext> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var sqliteConnectionString = _configuration.GetConnectionString("SmallMealPlan");
            _logger.LogInformation($"Using Sqlite connection string: {sqliteConnectionString}");
            optionsBuilder.UseSqlite(sqliteConnectionString);
        }
    }
}