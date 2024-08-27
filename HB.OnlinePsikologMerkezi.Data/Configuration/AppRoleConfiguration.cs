using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HB.OnlinePsikologMerkezi.Data.Configuration
{
    public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {

            //builder.HasData(new AppRole[]
            //{
            //    new(){Id=Guid.NewGuid().ToString(),Name="member"},
            //    new(){Id=Guid.NewGuid().ToString(),Name="admin"},
            //    new(){Id=Guid.NewGuid().ToString(),Name="psk"},
            //});

        }
    }
}
