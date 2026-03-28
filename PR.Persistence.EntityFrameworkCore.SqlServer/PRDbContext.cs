using Microsoft.EntityFrameworkCore;

namespace PR.Persistence.EntityFrameworkCore.SqlServer
{
    public class PRDbContext : PRDbContextBase
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConnectionStringProvider.GetConnectionString();
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
