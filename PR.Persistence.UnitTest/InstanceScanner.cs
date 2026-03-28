using StructureMap;

namespace PR.Persistence.UnitTest
{
    internal class InstanceScanner : Registry
    {
        public InstanceScanner()
        {
            Scan(_ =>
            {
                _.AssembliesAndExecutablesFromApplicationBaseDirectory(); // The implementation of the interface is in another assembly in the base directory
                _.WithDefaultConventions(); // Default Convention is that the interface has the same name as the implementation except for the I prefix
            });
        }
    }
}