using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Data.Respository;
using HB.OnlinePsikologMerkezi.Entities.Interface;


namespace HB.OnlinePsikologMerkezi.Data.UnitOfWork
{
    public class Uow : IUow
    {
        private readonly AppDbContext context;

        public Uow(AppDbContext context)
        {
            this.context = context;
        }

        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
        }

        public IRepository<T> GetRepository<T>() where T : class, IBaseEntity, new()
        {
            return new Repository<T>(context);
        }
    }
}
