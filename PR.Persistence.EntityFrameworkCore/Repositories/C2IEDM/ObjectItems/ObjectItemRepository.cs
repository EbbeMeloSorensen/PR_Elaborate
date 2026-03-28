using Microsoft.EntityFrameworkCore;
using Craft.Persistence.EntityFrameworkCore;
using PR.Domain.Entities.C2IEDM.ObjectItems;
using PR.Persistence.Repositories.C2IEDM.ObjectItems;

namespace PR.Persistence.EntityFrameworkCore.Repositories.C2IEDM.ObjectItems
{
    public class ObjectItemRepository : Repository<ObjectItem>, IObjectItemRepository
    {
        public ObjectItemRepository(DbContext context) : base(context)
        {
        }

        public override Task Clear()
        {
            throw new NotImplementedException();
        }

        public override Task Update(ObjectItem entity)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRange(IEnumerable<ObjectItem> entities)
        {
            throw new NotImplementedException();
        }
    }
}
