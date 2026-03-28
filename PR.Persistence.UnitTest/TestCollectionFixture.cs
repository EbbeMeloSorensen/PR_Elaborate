using StructureMap;

namespace PR.Persistence.UnitTest;

public class TestCollectionFixture : IDisposable
{
    public TestCollectionFixture()
    {
        // Initialization logic here (runs once before any tests in the collection)

        // Her kunne man f.eks. droppe databasen
    }

    public void Dispose()
    {
        // Cleanup logic here (runs once after all tests in the collection)
    }
}