using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Craft.Logging;
using Craft.Persistence;
using PR.Domain.Entities.PR;

namespace PR.Persistence.Repositories.PR
{
    public interface IPersonAssociationRepository : IRepository<PersonAssociation>
    {
        ILogger Logger { get; }

        Task<PersonAssociation> Get(
            Guid id);

        Task<IEnumerable<PersonAssociation>> GetAllVariants(
            Guid id);
    }
}