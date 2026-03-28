using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PR.Domain.Entities.PR;

namespace PR.Persistence.EntityFrameworkCore.EntityConfigurations.PR
{
    public class PersonCommentConfiguration : IEntityTypeConfiguration<PersonComment>
    {
        private bool _versioned;

        public PersonCommentConfiguration(
            bool versioned)
        {
            _versioned = versioned;
        }

        public void Configure(
            EntityTypeBuilder<PersonComment> builder)
        {
            if (_versioned)
            {
                builder.HasKey(p => p.ArchiveID);
            }
            else
            {
                builder.HasKey(p => p.ID);
            }
        }
    }
}
