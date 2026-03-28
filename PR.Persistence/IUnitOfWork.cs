using System;
using PR.Persistence.Repositories.PR;
using PR.Persistence.Repositories.Smurfs;

namespace PR.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        ISmurfRepository Smurfs { get; }

        IPersonRepository People { get; }
        IPersonCommentRepository PersonComments { get; }
        IPersonAssociationRepository PersonAssociations { get; }

        void Clear();

        void Complete();
    }
}
