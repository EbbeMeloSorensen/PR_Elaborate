using StructureMap;
using System.Configuration;

namespace PR.UI.WPF.GIS
{
    public class MainWindowViewModelRegistry : Registry
    {
        public MainWindowViewModelRegistry()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var repositoryPluginAssembly = settings["RepositoryPluginAssembly"]?.Value;

            Scan(_ =>
            {
                _.WithDefaultConventions();
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("Craft.Logging"));
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("Craft.UIElements"));
                _.AssembliesFromApplicationBaseDirectory(d => d.FullName.StartsWith("PR"));
                _.Assembly(repositoryPluginAssembly);
                _.LookForRegistries();
            });
        }
    }
}