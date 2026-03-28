using System.Configuration;
using PR.Persistence.Versioned;
using StructureMap;
using PR.ViewModel;
using PR.Persistence;

namespace PR.UI.WPF
{
    public class BootStrapper
    {
        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                try
                {
                    var mainWindowViewModel = Container.For<MainWindowViewModelRegistry>().GetInstance<MainWindowViewModel>();

                    var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var settings = configFile.AppSettings.Settings;
                    var versioning = settings["Versioning"]?.Value;
                    var reseeding = settings["Reseeding"]?.Value;

                    if (versioning == "enabled")
                    {
                        // Den skal ikke wrappes, hvis det er en af dem, der repræsenterer et API
                        if (mainWindowViewModel.UnitOfWorkFactory is not IUnitOfWorkFactoryVersioned)
                        {
                            // Wrap the UnitOfWorkFactory, so we get versioning and history
                            mainWindowViewModel.UnitOfWorkFactory =
                                new UnitOfWorkFactoryFacade(mainWindowViewModel.UnitOfWorkFactory);
                        }
                    }

                    mainWindowViewModel.Initialize(
                        versioning == "enabled",
                        reseeding == "enabled");

                    return mainWindowViewModel;
                }
                catch (ConfigurationErrorsException)
                {
                    System.Console.WriteLine("Error reading app settings");
                    throw;
                }
            }
        }
    }
}
