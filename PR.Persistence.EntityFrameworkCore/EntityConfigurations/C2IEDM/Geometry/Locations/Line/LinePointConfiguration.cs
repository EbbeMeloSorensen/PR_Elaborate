using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PR.Domain.Entities.C2IEDM.Geometry;

namespace PR.Persistence.EntityFrameworkCore.EntityConfigurations.C2IEDM.Geometry.Locations.Line
{
    public class LinePointConfiguration : IEntityTypeConfiguration<LinePoint>
    {
        public void Configure(EntityTypeBuilder<LinePoint> builder)
        {
            builder.HasKey(lp => new { lp.LineID, lp.Index });
        }
    }
}
