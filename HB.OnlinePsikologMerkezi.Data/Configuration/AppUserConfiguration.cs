using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HB.OnlinePsikologMerkezi.Data.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(30);
            builder.Property(x => x.LastName).HasMaxLength(30);

            builder.HasOne(x => x.Psychologist).WithOne(x => x.AppUser).HasForeignKey<Psychologist>(x => x.Psychologist_ID);


            //telefon numarasını şifreleyerek saklamak
            //daha sonra kaldırılabilir duruma göre
            //builder.Property(x => x.PhoneNumber).HasConversion(a => CustomEncryption.Encrypt(a), a => CustomEncryption.Decrypt(a));

        }

    }
}
