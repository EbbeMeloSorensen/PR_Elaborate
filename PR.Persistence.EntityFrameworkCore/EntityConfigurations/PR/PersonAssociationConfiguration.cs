using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PR.Domain.Entities.PR;

namespace PR.Persistence.EntityFrameworkCore.EntityConfigurations.PR;

public class PersonAssociationConfiguration : IEntityTypeConfiguration<PersonAssociation>
{
    private bool _versioned;

    public PersonAssociationConfiguration(
        bool versioned)
    {
        _versioned = versioned;
    }

    public void Configure(
        EntityTypeBuilder<PersonAssociation> builder)
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