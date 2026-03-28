using Xunit;

namespace PR.Application.UnitTest
{
    [CollectionDefinition("Test Collection 1")]
    public class TestCollection1 : ICollectionFixture<TestCollectionFixture>
    {
        // This class is empty. Its sole purpose is to attach the fixture to the test collection.
    }

    [CollectionDefinition("Test Collection 2")]
    public class TestCollection2 : ICollectionFixture<TestCollectionFixture>
    {
        // This class is empty. Its sole purpose is to attach the fixture to the test collection.
    }
}