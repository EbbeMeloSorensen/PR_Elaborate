using Microsoft.EntityFrameworkCore;
using Craft.Persistence.EntityFrameworkCore;
using PR.Domain.Entities.Smurfs;
using PR.Persistence.Repositories.Smurfs;

namespace PR.Persistence.EntityFrameworkCore.Repositories.Smurfs
{
    public class SmurfRepository : Repository<Smurf>, ISmurfRepository
    {
        private PRDbContextBase PrDbContext => Context as PRDbContextBase;

        public SmurfRepository(DbContext context) : base(context)
        {
        }

        public override async Task Clear()
        {
            Context.RemoveRange(PrDbContext.Smurfs);
            await Context.SaveChangesAsync();
        }

        public override Task Update(Smurf entity)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateRange(IEnumerable<Smurf> entities)
        {
            throw new NotImplementedException();
        }
    }
}
