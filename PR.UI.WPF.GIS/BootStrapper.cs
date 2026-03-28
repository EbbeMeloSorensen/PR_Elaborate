using StructureMap;
using System;
using System.Configuration;
using PR.Persistence.Versioned;
using PR.ViewModel.GIS;
using PR.Persistence;

namespace PR.UI.WPF.GIS
{
    public class BootStrapper
    {
        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                // Det her burde man kunne genbruge - det er det samme som i bootstrapperen for det andet WPF proj
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
                    Console.WriteLine("Error reading app settings");
                    throw;
                }
            }
        }
    }
}
