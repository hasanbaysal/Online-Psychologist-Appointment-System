using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HB.OnlinePsikologMerkezi.Data.Configuration
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasOne(x => x.Psychologist).WithMany(x => x.Appointments).HasForeignKey(x => x.PsychologistId);

            builder.HasOne(x => x.Order).WithOne(x => x.Appointment).HasForeignKey<Order>(x => x.AppointmentId);

            builder.Property(x => x.Status).IsConcurrencyToken();


            builder.Property(x => x.Start3DTime).IsRequired(false);    
        }
    }
}
