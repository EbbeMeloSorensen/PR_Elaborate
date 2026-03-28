using PR.Persistence.APIClient.DFOS.Repositories;
using System;
using Craft.Logging;
using PR.Persistence.Repositories.PR;
using PR.Persistence.Repositories.Smurfs;

namespace PR.Persistence.APIClient.DFOS
{
    public class UnitOfWork : IUnitOfWork
    {
        public ISmurfRepository Smurfs { get; }

        public IPersonRepository People { get; }
        public IPersonCommentRepository PersonComments { get; }
        public IPersonAssociationRepository PersonAssociations { get; }


        public UnitOfWork(
            ILogger logger,
            string baseURL,
            DateTime? historicalTime,
            DateTime? databaseTime)
        {
            People = new PersonRepository(
                logger,
                baseURL,
                historicalTime,
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
