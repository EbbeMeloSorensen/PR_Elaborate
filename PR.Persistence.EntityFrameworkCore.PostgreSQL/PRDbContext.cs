using Microsoft.EntityFrameworkCore;

namespace PR.Persistence.EntityFrameworkCore.PostgreSQL
{
    public class PRDbContext : PRDbContextBase
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConnectionStringProvider.GetConnectionString();
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
