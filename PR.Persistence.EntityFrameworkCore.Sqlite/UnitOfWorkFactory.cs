using Craft.Logging;

namespace PR.Persistence.EntityFrameworkCore.Sqlite
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public ILogger Logger { get; set; }

        public void Initialize(
            bool versioned)
        {
            PRDbContextBase.Versioned = versioned;
            using var context = new PRDbContext();
            context.Database.EnsureCreated();
            Seeding.SeedDatabase(context);
        }

        public void OverrideConnectionString(
            string connectionString)
        {
            ConnectionStringProvider.ConnectionString = connectionString;
        }

        public IUnitOfWork GenerateUnitOfWork()
        {
            return new UnitOfWork(new PRDbContext());
        }

        public void Reseed()
        {
            using var context = new PRDbContext();
            context.Database.EnsureCreated();

            using var unitOfWork = GenerateUnitOfWork();
            unitOfWork.Clear();
            unitOfWork.Complete();

            Seeding.SeedDatabase(context);
            unitOfWork.Complete();
        }
    }
}
