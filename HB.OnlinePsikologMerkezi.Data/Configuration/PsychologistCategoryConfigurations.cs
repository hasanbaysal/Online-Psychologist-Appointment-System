using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HB.OnlinePsikologMerkezi.Data.Configuration
{
    public class PsychologistCategoryConfigurations : IEntityTypeConfiguration<PsychologistCategory>
    {
        public void Configure(EntityTypeBuilder<PsychologistCategory> builder)
        {
            builder.HasKey(x => new { x.PsycholoistId, x.CategorId });


            builder.HasOne(x => x.Psychologist)
                .WithMany(x => x.PsychologistCategories)
                .HasForeignKey(x => x.PsycholoistId);


            builder.HasOne(x => x.Category)
                .WithMany(x => x.PsychologistCategories)
                .HasForeignKey(x => x.CategorId);
        }
    }
}
