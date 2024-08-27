using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HB.OnlinePsikologMerkezi.Data.Configuration
{
    public class OrderConfiguraiton : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasOne(x => x.AppUser).WithMany(x => x.Orders).HasForeignKey(x => x.AppUserId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
