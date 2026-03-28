using System;
using PR.Persistence.APIClient.Repositories;
using PR.Persistence.Repositories.PR;
using PR.Persistence.Repositories.Smurfs;

namespace PR.Persistence.APIClient
{
    public class UnitOfWork : IUnitOfWork
    {
        public ISmurfRepository Smurfs { get; }

        public IPersonRepository People { get; }
        public IPersonCommentRepository PersonComments { get; }
        public IPersonAssociationRepository PersonAssociations { get; }

        public UnitOfWork(
            string baseURL,
            DateTime? historicalTime,
            bool includeHistoricalObjects,
            DateTime? databaseTime)
        {
            People = new PersonRepository(
                baseURL,
                historicalTime,
                includeHistoricalObjects,
                databaseTime);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Complete()
        {
        }

        public void Dispose()
        {
        }
    }
}
