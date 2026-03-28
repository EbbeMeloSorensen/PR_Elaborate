using Microsoft.EntityFrameworkCore;

namespace PR.Persistence.EntityFrameworkCore.InMemory
{
    public class PRDbContext : PRDbContextBase
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Dummy");
        }
    }
}