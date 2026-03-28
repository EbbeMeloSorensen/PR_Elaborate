using System.Configuration;
using StructureMap;

namespace PR.UI.Console
{
    internal class InstanceScanner : Registry
    {
        public InstanceScanner()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var repositoryPluginAssembly = settings["RepositoryPluginAssembly"]?.Value;

            Scan(_ =>
            {
                _.WithDefaultConventions();
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("Craft.Logging"));
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("PR.Domain"));
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("PR.IO"));
                _.Assembly(repositoryPluginAssembly);
                _.LookForRegistries();
            });
        }
    }
}
