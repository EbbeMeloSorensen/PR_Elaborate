using System;
using System.Configuration;
using Craft.Logging;

namespace PR.Persistence.APIClient.DFOS
{
    public class UnitOfWorkFactory : IUnitOfWorkFactoryVersioned, IUnitOfWorkFactoryHistorical
    {
        private static string _baseURL;

        public DateTime? HistoricalTime { get; set; }
        public bool IncludeCurrentObjects { get; set; }
        public bool IncludeHistoricalObjects { get; set; }
        public DateTime? DatabaseTime { get; set; }

        static UnitOfWorkFactory()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            _baseURL = settings["BaseURL"]?.Value;
        }

        public ILogger Logger { get; set; }

        public void Initialize(
            bool versioned)
        {
            // Here we would normally have made sure the database existed and possibly seeded it,
            // but we don't do anything, when the unit of work represents an API

            // We might obtain the token here, though..
        }

        public void OverrideConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork GenerateUnitOfWork()
        {
            return new UnitOfWork(
                Logger,
                _baseURL,
                HistoricalTime,
                DatabaseTime);
        }

        public void Reseed()
        {
            throw new System.NotImplementedException();
        }
    }
}
