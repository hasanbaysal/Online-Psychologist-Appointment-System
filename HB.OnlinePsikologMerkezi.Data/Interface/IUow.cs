using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Data.Interface
{
    public interface IUow
    {
        Task CommitAsync();
        IRepository<T> GetRepository<T>() where T : class, IBaseEntity, new();
    }
}
