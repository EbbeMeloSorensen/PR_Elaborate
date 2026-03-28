using Microsoft.EntityFrameworkCore;

namespace PR.Persistence.EntityFrameworkCore.Sqlite
{
    public class PRDbContext : PRDbContextBase
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionStringProvider.ConnectionString);
        }
    }
}
