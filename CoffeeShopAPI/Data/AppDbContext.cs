using Microsoft.EntityFrameworkCore;

namespace CoffeeShopAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        public DbSet<Models.Order> Orders { get; set; }
    }
}
