using System.Collections.Generic;
using Craft.Persistence;
using PR.Domain.Entities.C2IEDM.Geometry;

namespace PR.Persistence.Repositories.C2IEDM.Geometry
{
    public interface ILineRepository : IRepository<Line>
    {
        IList<Line> GetLinesIncludingPoints();
    }
}
