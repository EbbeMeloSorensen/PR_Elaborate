using Craft.Logging;

namespace PR.Persistence
{
    public interface IUnitOfWorkFactory
    {
        ILogger Logger { get; set; }

        void Initialize(
            bool versioned);

        void OverrideConnectionString(
            string connectionString);

        IUnitOfWork GenerateUnitOfWork();

        void Reseed();
    }
}
